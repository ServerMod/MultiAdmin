using System;//why is a bullet here?
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MultiAdmin.MultiAdmin;

namespace MultiAdmin
{
	class OutputThread
	{
		public static readonly Regex SMOD_REGEX = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
		public static readonly ConsoleColor DEFAULT_FOREGROUND = ConsoleColor.Cyan;
		public static readonly ConsoleColor DEFAULT_BACKGROUND = ConsoleColor.Black;

		public static ConsoleColor MapConsoleColor(string color, ConsoleColor def = ConsoleColor.Cyan)
		{
			try
			{
				return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
			}
			catch
			{
				return def;
			}
		}

		public static void Read(Server server)
		{
			string dedicatedDir = "SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated";
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = dedicatedDir;
			watcher.IncludeSubdirectories = true;

			if (isLinux())
			{
				ReadLinux(server, watcher);
				return;
			}

			ReadWindows(server, watcher);
		}

		public static Boolean isLinux()
		{
			int p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128);
		}

		public static void ReadWindows(Server server, FileSystemWatcher watcher)
		{
			watcher.Changed += new FileSystemEventHandler((sender, eventArgs) => OnDirectoryChanged(sender, eventArgs, server));
			watcher.EnableRaisingEvents = true;
		}

		public static void ReadLinux(Server server, FileSystemWatcher watcher)
		{
			watcher.Created += new FileSystemEventHandler((sender, eventArgs) => OnMapiCreated(sender, eventArgs, server));
			watcher.Filter = "sl*.mapi";
			watcher.EnableRaisingEvents = true;
		}

		private static void OnDirectoryChanged(object source, FileSystemEventArgs e, Server server)
		{
			if (!Directory.Exists(e.FullPath))
			{
				return;
			}

			if (!e.FullPath.Contains(server.GetSessionId()))
			{
				return;
			}

			string[] files = Directory.GetFiles(e.FullPath, "sl*.mapi", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray<string>();
			foreach (string file in files)
			{
				OutputThread.ProcessFile(server, file);
			}
		}

		private static void OnMapiCreated(object source, FileSystemEventArgs e, Server server)
		{
			if (!e.FullPath.Contains(server.GetSessionId()))
			{
				return;
			}

			Thread.Sleep(15);
			OutputThread.ProcessFile(server, e.FullPath);
		}

		private static void ProcessFile(Server server, string file)
		{
			string stream = string.Empty;
			string command = "open";
			int attempts = 0;
			bool read = false;

			while (attempts < (server.runOptimized ? 10 : 100) && !read && !server.IsStopping())
			{
				try
				{
					if (!File.Exists(file))
					{
						// The file definitely existed at the moment Change event was raised by OS
						// If the file is not here after 15 ms that means that
						// (a) either it was already processed
						// (b) it was deleted by some other application
						return;
					}

					StreamReader sr = new StreamReader(file);
					stream = sr.ReadToEnd();
					command = "close";
					sr.Close();
					command = "delete";
					File.Delete(file);
					read = true;
				}
				catch
				{
					attempts++;
					if (attempts >= (server.runOptimized ? 10 : 100))
					{
						server.Write("Message printer warning: Could not " + command + " " + file + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow);
						server.Write("skipping");
					}
				}
			}

			if (server.IsStopping()) return;

			bool display = true;
			ConsoleColor color = ConsoleColor.Cyan;

			if (!string.IsNullOrEmpty(stream.Trim()))
			{
				if (stream.Contains("LOGTYPE"))
				{
					string type = stream.Substring(stream.IndexOf("LOGTYPE")).Trim();
					stream = stream.Substring(0, stream.IndexOf("LOGTYPE")).Trim();

					switch (type)
					{
						case "LOGTYPE02":
							color = ConsoleColor.Green;
							break;
						case "LOGTYPE-8":
							color = ConsoleColor.DarkRed;
							break;
						case "LOGTYPE14":
							color = ConsoleColor.Magenta;
							break;
						default:
							color = ConsoleColor.Cyan;
							break;
					}
				}
			}

			string[] streamSplit;

			// Smod2 loggers pretty printing
			var match = SMOD_REGEX.Match(stream);
			if (match.Success)
			{
				if (match.Groups.Count >= 2)
				{
					ConsoleColor levelColor = ConsoleColor.Cyan;
					ConsoleColor tagColor = ConsoleColor.Yellow;
					ConsoleColor msgColor = ConsoleColor.White;
					switch (match.Groups[1].Value.Trim())
					{
						case "DEBUG":
							levelColor = ConsoleColor.Gray;
							break;
						case "INFO":
							levelColor = ConsoleColor.Green;
							break;
						case "WARN":
							levelColor = ConsoleColor.DarkYellow;
							break;
						case "ERROR":
							levelColor = ConsoleColor.Red;
							msgColor = ConsoleColor.Red;
							break;
						default:
							color = ConsoleColor.Cyan;
							break;
					}

					lock (server)
					{
						server.WritePart(string.Empty, DEFAULT_BACKGROUND, ConsoleColor.Cyan, true, false);
						server.WritePart("[" + match.Groups[1].Value + "] ", DEFAULT_BACKGROUND, levelColor, false, false);
						server.WritePart(match.Groups[2].Value + " ", DEFAULT_BACKGROUND, tagColor, false, false);
						server.WritePart(match.Groups[3].Value, DEFAULT_BACKGROUND, msgColor, false, true);
					}

					server.Log("[" + match.Groups[1].Value + "] " + match.Groups[2].Value + " " + match.Groups[3].Value);

					// P.S. the format is [Info] [courtney.exampleplugin] Something interesting happened
					// That was just an example
					display = false;

					// Limiting output speed for Smod messages
					Thread.Sleep(server.printSpeed);

					// This return should be here
					return;
				}
			}

			if (stream.Contains("Mod Log:"))
			{
				foreach (Feature f in server.Features)
				{
					if (f is IEventAdminAction)
					{
						((IEventAdminAction)f).OnAdminAction(stream.Replace("Mod log:", string.Empty));
					}
				}
			}

			if (stream.Contains("ServerMod - Version"))
			{
				server.HasServerMod = true;
				// This should work fine with older ServerMod versions too
				streamSplit = stream.Replace("ServerMod - Version", string.Empty).Split('-');
				server.ServerModVersion = streamSplit[0].Trim();
				server.ServerModBuild = (streamSplit.Length > 1 ? streamSplit[1] : "A").Trim();
			}

			if (server.ServerModCheck(1, 7, 2))
			{
				if (stream.Contains("Round restarting"))
				{
					foreach (Feature f in server.Features)
					{
						if (f is IEventRoundEnd)
						{
							((IEventRoundEnd)f).OnRoundEnd();
						}
					}
				}

				if (stream.Contains("Waiting for players"))
				{
					if (!server.InitialRoundStarted)
					{
						server.InitialRoundStarted = true;
						foreach (Feature f in server.Features)
						{
							if (f is IEventRoundStart)
							{
								((IEventRoundStart)f).OnRoundStart();
							}
						}
					}

					if (server.ServerModCheck(1, 5, 0) && server.fixBuggedPlayers)
					{
						server.SendMessage("ROUNDRESTART");
						server.fixBuggedPlayers = false;
					}
				}
			}
			else
			{
				if (stream.Contains("Waiting for players"))
				{
					if (!server.InitialRoundStarted)
					{
						server.InitialRoundStarted = true;
						foreach (Feature f in server.Features)
						{
							if (f is IEventRoundStart)
							{
								((IEventRoundStart)f).OnRoundStart();
							}
						}
					}
					else
					{
						foreach (Feature f in server.Features)
						{
							if (f is IEventRoundEnd)
							{
								((IEventRoundEnd)f).OnRoundEnd();
							}
						}
					}

					if (server.ServerModCheck(1, 5, 0) && server.fixBuggedPlayers)
					{
						server.SendMessage("ROUNDRESTART");
						server.fixBuggedPlayers = false;
					}
				}
			}



			if (stream.Contains("New round has been started"))
			{

				foreach (Feature f in server.Features)
				{
					if (f is IEventRoundStart)
					{
						((IEventRoundStart)f).OnRoundStart();
					}
				}
			}

			if (stream.Contains("Level loaded. Creating match..."))
			{
				foreach (Feature f in server.Features)
				{
					if (f is IEventServerStart)
					{
						((IEventServerStart)f).OnServerStart();
					}
				}
			}


			if (stream.Contains("Server full"))
			{
				foreach (Feature f in server.Features)
				{
					if (f is IEventServerFull)
					{
						((IEventServerFull)f).OnServerFull();
					}
				}
			}


			if (stream.Contains("Player connect"))
			{
				display = false;
				server.Log("Player connect event");
				foreach (Feature f in server.Features)
				{
					if (f is IEventPlayerConnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						((IEventPlayerConnect)f).OnPlayerConnect(name);
					}
				}
			}

			if (stream.Contains("Player disconnect"))
			{
				display = false;
				server.Log("Player disconnect event");
				foreach (Feature f in server.Features)
				{
					if (f is IEventPlayerDisconnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						((IEventPlayerDisconnect)f).OnPlayerDisconnect(name);
					}
				}
			}

			if (stream.Contains("Player has connected before load is complete"))
			{
				if (server.ServerModCheck(1, 5, 0))
				{
					server.fixBuggedPlayers = true;
				}
			}

			if (stream.Contains("ServerMod successfully validated config file"))
			{
				display = false;
				server.Log("ServerMod successfully validated config file");

				if (server.GetMultimode())
				{
					bool success = true;
					var data = File.ReadAllText(server.MainConfigLocation);
					try
					{
						File.WriteAllText(server.ServerConfig.GetConfigFilePath(), data);
					}
					catch (Exception e)
					{
						success = false;
						server.Log(string.Format("Error: {0}", e.ToString()));
					}

					if (success)
					{
						server.Log(string.Format("MultiAdmin saved new config to {0}", server.ServerConfig.GetConfigFilePath()));
					}
					else
					{
						server.Log(string.Format("MultiAdmin failed saving new config to {0}", server.ServerConfig.GetConfigFilePath()));
					}
				}
				else
				{
					server.Log("MultiAdmin is running in single server mode. No further action required.");
				}
			}

			if (display)
			{
				server.Write(stream.Trim(), color);

				// Limiting output speed for generic message
				Thread.Sleep(server.printSpeed);
			}
		}
	}
}

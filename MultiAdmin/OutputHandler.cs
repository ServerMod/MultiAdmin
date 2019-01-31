using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MultiAdmin
{
	internal class OutputHandler : IDisposable
	{
		public static readonly Regex SmodRegex =
			new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);

		public const ConsoleColor DefaultBackground = ConsoleColor.Black;

		public static readonly string DedicatedDir = "SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated";

		private readonly FileSystemWatcher fsWatcher;
		private bool fixBuggedPlayers;

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

		public OutputHandler(Server server)
		{
			fsWatcher = new FileSystemWatcher { Path = DedicatedDir, IncludeSubdirectories = true };

			if (Utils.IsUnix)
			{
				ReadLinux(server, fsWatcher);
				return;
			}

			ReadWindows(server, fsWatcher);
		}

		private void ReadWindows(Server server, FileSystemWatcher watcher)
		{
			watcher.Changed += (sender, eventArgs) => OnDirectoryChanged(eventArgs, server);
			watcher.EnableRaisingEvents = true;
		}

		private void ReadLinux(Server server, FileSystemWatcher watcher)
		{
			watcher.Created += (sender, eventArgs) => OnMapiCreated(eventArgs, server);
			watcher.Filter = "sl*.mapi";
			watcher.EnableRaisingEvents = true;
		}

		private void OnDirectoryChanged(FileSystemEventArgs e, Server server)
		{
			if (!Directory.Exists(e.FullPath)) return;

			if (server.Stopping || !e.FullPath.Contains(server.SessionId)) return;

			string[] files = Directory.GetFiles(e.FullPath, "sl*.mapi", SearchOption.TopDirectoryOnly).OrderBy(f => f)
				.ToArray();
			foreach (string file in files) ProcessFile(server, file);
		}

		private void OnMapiCreated(FileSystemEventArgs e, Server server)
		{
			if (server.Stopping || !e.FullPath.Contains(server.SessionId)) return;

			Thread.Sleep(15);
			ProcessFile(server, e.FullPath);
		}

		private void ProcessFile(Server server, string file)
		{
			string stream = string.Empty;
			string command = "open";
			int attempts = 0;
			bool read = false;

			while (attempts < 50 && !read && !server.Stopping)
				try
				{
					if (!File.Exists(file)) return;

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
					if (attempts >= 50)
					{
						server.Write(
							"Message printer warning: Could not " + command + " " + file +
							". Make sure that MultiAdmin.exe has all necessary read-write permissions.");
						server.Write("skipping");
					}
				}

			if (server.Stopping) return;

			bool display = true;
			ConsoleColor color = ConsoleColor.Cyan;

			if (!string.IsNullOrEmpty(stream.Trim()))
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

			// Smod2 loggers pretty printing
			Match match = SmodRegex.Match(stream);
			if (match.Success)
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
						server.WritePart(string.Empty, DefaultBackground, ConsoleColor.Cyan, true, false);
						server.WritePart("[" + match.Groups[1].Value + "] ", DefaultBackground, levelColor, false, false);
						server.WritePart(match.Groups[2].Value + " ", DefaultBackground, tagColor, false, false);
						server.WritePart(match.Groups[3].Value, DefaultBackground, msgColor, false, true);
					}

					server.Log("[" + match.Groups[1].Value + "] " + match.Groups[2].Value + " " + match.Groups[3].Value);

					// P.S. the format is [Info] [courtney.exampleplugin] Something interesting happened
					// That was just an example

					// This return should be here
					return;
				}

			if (stream.Contains("Mod Log:"))
				foreach (Feature f in server.features)
					if (f is IEventAdminAction adminAction)
						adminAction.OnAdminAction(stream.Replace("Mod log:", string.Empty));

			if (stream.Contains("ServerMod - Version"))
			{
				server.hasServerMod = true;
				// This should work fine with older ServerMod versions too
				string[] streamSplit = stream.Replace("ServerMod - Version", string.Empty).Split('-');
				server.serverModVersion = streamSplit[0].Trim();
				server.serverModBuild = (streamSplit.Length > 1 ? streamSplit[1] : "A").Trim();
			}

			if (stream.Contains("Round restarting"))
				foreach (Feature f in server.features)
					if (f is IEventRoundEnd roundEnd)
						roundEnd.OnRoundEnd();

			if (stream.Contains("Waiting for players"))
			{
				if (!server.initialRoundStarted)
				{
					server.initialRoundStarted = true;
					foreach (Feature f in server.features)
						if (f is IEventRoundStart roundStart)
							roundStart.OnRoundStart();
				}

				if (fixBuggedPlayers)
				{
					server.SendMessage("ROUNDRESTART");
					fixBuggedPlayers = false;
				}
			}


			if (stream.Contains("New round has been started"))
				foreach (Feature f in server.features)
					if (f is IEventRoundStart roundStart)
						roundStart.OnRoundStart();

			if (stream.Contains("Level loaded. Creating match..."))
				foreach (Feature f in server.features)
					if (f is IEventServerStart serverStart)
						serverStart.OnServerStart();


			if (stream.Contains("Server full"))
				foreach (Feature f in server.features)
					if (f is IEventServerFull serverFull)
						serverFull.OnServerFull();


			if (stream.Contains("Player connect"))
			{
				display = false;
				server.Log("Player connect event");
				foreach (Feature f in server.features)
					if (f is IEventPlayerConnect playerConnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						playerConnect.OnPlayerConnect(name);
					}
			}

			if (stream.Contains("Player disconnect"))
			{
				display = false;
				server.Log("Player disconnect event");
				foreach (Feature f in server.features)
					if (f is IEventPlayerDisconnect playerDisconnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						playerDisconnect.OnPlayerDisconnect(name);
					}
			}

			if (stream.Contains("Player has connected before load is complete"))
				fixBuggedPlayers = true;

			if (display) server.Write(stream.Trim(), color);
		}

		public void Dispose()
		{
			fsWatcher?.Dispose();
		}
	}
}
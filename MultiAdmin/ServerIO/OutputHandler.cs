using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.ServerIO
{
	public class OutputHandler : IDisposable
	{
		public static readonly Regex SmodRegex =
			new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);

		public const ConsoleColor DefaultBackground = ConsoleColor.Black;

		private readonly FileSystemWatcher fsWatcher;
		private bool fixBuggedPlayers;

		public static ConsoleColor MapConsoleColor(string color, ConsoleColor def = ConsoleColor.Cyan)
		{
			try
			{
				return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
			}
			catch (Exception e)
			{
				Program.LogDebugException("MapConsoleColor", e);
				return def;
			}
		}

		public OutputHandler(Server server)
		{
			if (server == null)
			{
				Program.Write("Error in OutputHandler - Server server is null!", ConsoleColor.Red);
				return;
			}

			if (string.IsNullOrEmpty(server.SessionDirectory))
			{
				server.Write($"Missing session directory! Output is not being watched... (SessionDirectory = \"{server.SessionDirectory ?? "null"}\" SessionId = \"{server.SessionId ?? "null"}\" DedicatedDir = \"{Server.DedicatedDir ?? "null"}\")", ConsoleColor.Red);
				return;
			}

			fsWatcher = new FileSystemWatcher {Path = server.SessionDirectory};

			fsWatcher.Created += (sender, eventArgs) => OnMapiCreated(eventArgs, server);
			fsWatcher.Filter = "sl*.mapi";
			fsWatcher.EnableRaisingEvents = true;
		}

		/* Old Windows MAPI Watching Code
		private void OnDirectoryChanged(FileSystemEventArgs e, Server server)
		{
			if (!Directory.Exists(e.FullPath)) return;

			string[] files = Directory.GetFiles(e.FullPath, "sl*.mapi", SearchOption.TopDirectoryOnly).OrderBy(f => f)
				.ToArray();
			foreach (string file in files) ProcessFile(server, file);
		}
		*/

		private void OnMapiCreated(FileSystemEventArgs e, Server server)
		{
			if (!File.Exists(e.FullPath)) return;

			try
			{
				ProcessFile(server, e.FullPath);
			}
			catch (Exception ex)
			{
				Program.LogDebugException("OnMapiCreated", ex);
			}
		}

		private void ProcessFile(Server server, string file)
		{
			string stream = string.Empty;
			string command = "open";

			bool isRead = false;

			// Lock this object to wait for this event to finish before trying to read another file
			lock (this)
			{
				for (int attempts = 0; attempts < 100; attempts++)
				{
					try
					{
						if (!File.Exists(file)) return;

						// Lock the file to prevent it from being modified further, or read by another instance
						using (StreamReader sr = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None)))
						{
							command = "read";
							stream = sr.ReadToEnd();

							isRead = true;
						}

						command = "delete";
						File.Delete(file);

						break;
					}
					catch (UnauthorizedAccessException e)
					{
						Program.LogDebugException("ProcessFile", e);
						Thread.Sleep(5);
					}
					catch (Exception e)
					{
						Program.LogDebugException("ProcessFile", e);
						Thread.Sleep(2);
					}
				}
			}

			if (!isRead)
			{
				server.Write(
					$"Message printer warning: Could not {command} {file}. Make sure that MultiAdmin has all necessary read-write permissions.");
				server.Write("Skipping...");

				return;
			}

			bool display = true;
			ConsoleColor color = ConsoleColor.Cyan;

			if (stream.EndsWith(Environment.NewLine))
				stream = stream.Substring(0, stream.Length - Environment.NewLine.Length);

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
			{
				if (match.Groups.Count >= 3)
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

					server.Write(new ColoredMessage[]
					{
						new ColoredMessage($"[{match.Groups[1].Value}] ", levelColor, DefaultBackground),
						new ColoredMessage($"{match.Groups[2].Value} ", tagColor, DefaultBackground),
						new ColoredMessage(match.Groups[3].Value, msgColor, DefaultBackground)
					}, ConsoleColor.Cyan);

					// P.S. the format is [Info] [courtney.exampleplugin] Something interesting happened
					// That was just an example

					// This return should be here
					return;
				}
			}

			if (stream.Contains("Mod Log:"))
				foreach (Feature f in server.features)
					if (f is IEventAdminAction adminAction)
						adminAction.OnAdminAction(stream.Replace("Mod Log:", string.Empty));

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
				foreach (Feature f in server.features)
					if (f is IEventWaitingForPlayers waitingForPlayers)
						waitingForPlayers.OnWaitingForPlayers();

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
				{
					if (f is IEventPlayerConnect playerConnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						playerConnect.OnPlayerConnect(name);
					}
				}
			}

			if (stream.Contains("Player disconnect"))
			{
				display = false;
				server.Log("Player disconnect event");
				foreach (Feature f in server.features)
				{
					if (f is IEventPlayerDisconnect playerDisconnect)
					{
						string name = stream.Substring(stream.IndexOf(":"));
						playerDisconnect.OnPlayerDisconnect(name);
					}
				}
			}

			if (stream.Contains("Player has connected before load is complete"))
				fixBuggedPlayers = true;

			if (display) server.Write(stream, color);
		}

		public void Dispose()
		{
			fsWatcher?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}

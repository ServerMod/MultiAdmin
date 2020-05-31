using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public class OutputHandler
	{
		public static readonly Regex SmodRegex =
			new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);

		private bool fixBuggedPlayers;

		private readonly Server server;

		public OutputHandler(Server server)
		{
			this.server = server;
		}

		public static ConsoleColor MapConsoleColor(string color, ConsoleColor def = ConsoleColor.Cyan)
		{
			try
			{
				return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(MapConsoleColor), e);
				return def;
			}
		}

		public void HandleMessage(object source, string message)
		{
			if (message == null)
				return;

			bool display = true;
			ConsoleColor color = ConsoleColor.Cyan;

			if (message.Length > 0)
			{
				if (byte.TryParse(Convert.ToString(message[0]), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out byte consoleColor))
				{
					color = (ConsoleColor)consoleColor;
					message = message.Substring(1);
				}

				// Smod2 loggers pretty printing
				Match match = SmodRegex.Match(message);
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

						server.Write(
							new ColoredMessage[]
							{
								new ColoredMessage($"[{match.Groups[1].Value}] ", levelColor),
								new ColoredMessage($"{match.Groups[2].Value} ", tagColor),
								new ColoredMessage(match.Groups[3].Value, msgColor)
							}, ConsoleColor.Cyan);

						// P.S. the format is [Info] [courtney.exampleplugin] Something interesting happened
						// That was just an example

						// This return should be here
						return;
					}
				}

				if (message.Contains("Mod Log:"))
					server.ForEachHandler<IEventAdminAction>(adminAction =>
						adminAction.OnAdminAction(message.Replace("Mod Log:", string.Empty)));

				if (message.Contains("ServerMod - Version"))
				{
					server.hasServerMod = true;
					// This should work fine with older ServerMod versions too
					string[] streamSplit = message.Replace("ServerMod - Version", string.Empty).Split('-');

					if (!streamSplit.IsEmpty())
					{
						server.serverModVersion = streamSplit[0].Trim();
						server.serverModBuild = (streamSplit.Length > 1 ? streamSplit[1] : "A").Trim();
					}
				}

				if (message.Contains("Round restarting"))
					server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());

				if (message.Contains("Waiting for players"))
				{
					server.IsLoading = false;

					server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers =>
						waitingForPlayers.OnWaitingForPlayers());

					if (fixBuggedPlayers)
					{
						server.SendMessage("ROUNDRESTART");
						fixBuggedPlayers = false;
					}
				}

				if (message.Contains("New round has been started"))
					server.ForEachHandler<IEventRoundStart>(roundStart => roundStart.OnRoundStart());

				if (message.Contains("Level loaded. Creating match..."))
					server.ForEachHandler<IEventServerStart>(serverStart => serverStart.OnServerStart());

				if (message.Contains("Server full"))
					server.ForEachHandler<IEventServerFull>(serverFull => serverFull.OnServerFull());

				if (message.Contains("Player connect"))
				{
					display = false;
					server.Log("Player connect event");

					int index = message.IndexOf(":");
					if (index >= 0)
					{
						string name = message.Substring(index);
						server.ForEachHandler<IEventPlayerConnect>(playerConnect =>
							playerConnect.OnPlayerConnect(name));
					}
				}

				if (message.Contains("Player disconnect"))
				{
					display = false;
					server.Log("Player disconnect event");

					int index = message.IndexOf(":");
					if (index >= 0)
					{
						string name = message.Substring(index);
						server.ForEachHandler<IEventPlayerDisconnect>(playerDisconnect =>
							playerDisconnect.OnPlayerDisconnect(name));
					}
				}

				if (message.Contains("Player has connected before load is complete"))
					fixBuggedPlayers = true;
			}

			if (display) server.Write(message, color);
		}
	}
}

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public class OutputHandler
	{
		public static readonly Regex SmodRegex =
			new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
		public static readonly char[] TrimChars = { '.', ' ', '\t', '!', '?', ',' };
		public static readonly char[] EventSplitChars = new char[] {':'};

		private readonly Server server;

		public OutputHandler(Server server)
		{
			this.server = server;
		}

		public void HandleMessage(object source, string message)
		{
			if (message == null)
				return;

			ColoredMessage coloredMessage = new ColoredMessage(message, ConsoleColor.White);

			if (!coloredMessage.text.IsEmpty())
			{
				// Convert the first character to the corresponding color
				if (byte.TryParse(coloredMessage.text[0].ToString(), NumberStyles.HexNumber,
					NumberFormatInfo.CurrentInfo, out byte consoleColor))
				{
					coloredMessage.textColor = (ConsoleColor)consoleColor;
					coloredMessage.text = coloredMessage.text.Substring(1);
				}

				// Smod2 loggers pretty printing
				Match match = SmodRegex.Match(coloredMessage.text);
				if (match.Success)
				{
					if (match.Groups.Count >= 3)
					{
						ConsoleColor levelColor = ConsoleColor.Green;
						ConsoleColor tagColor = ConsoleColor.Yellow;
						ConsoleColor msgColor = coloredMessage.textColor ?? ConsoleColor.White;

						switch (match.Groups[1].Value.Trim())
						{
							case "DEBUG":
								levelColor = ConsoleColor.DarkGray;
								break;

							case "INFO":
								levelColor = ConsoleColor.Green;
								break;

							case "WARN":
								levelColor = ConsoleColor.DarkYellow;
								break;

							case "ERROR":
								levelColor = ConsoleColor.Red;
								break;
						}

						server.Write(
							new[]
							{
								new ColoredMessage($"[{match.Groups[1].Value}] ", levelColor),
								new ColoredMessage($"{match.Groups[2].Value} ", tagColor),
								new ColoredMessage(match.Groups[3].Value, msgColor)
							}, msgColor);

						// P.S. the format is [Info] [courtney.exampleplugin] Something interesting happened
						// That was just an example

						// This return should be here
						return;
					}
				}

				string lowerMessage = coloredMessage.text.ToLower();
				if (!server.supportedModFeatures.HasFlag(ModFeatures.CustomEvents))
				{
					switch (lowerMessage.Trim(TrimChars))
					{
						case "the round is about to restart! please wait":
							server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());
							break;

						case "waiting for players":
							server.IsLoading = false;

							server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers => waitingForPlayers.OnWaitingForPlayers());
							break;

						case "new round has been started":
							server.ForEachHandler<IEventRoundStart>(roundStart => roundStart.OnRoundStart());
							break;

						case "level loaded. creating match":
							server.ForEachHandler<IEventServerStart>(serverStart => serverStart.OnServerStart());
							break;

						case "server full":
							server.ForEachHandler<IEventServerFull>(serverFull => serverFull.OnServerFull());
							break;
					}
				}

				if (lowerMessage.StartsWith("multiadmin:"))
				{
					// 11 chars in "multiadmin:"
					string eventMessage = coloredMessage.text.Substring(11);

					// Split event and event data
					string[] eventSplit = eventMessage.Split(EventSplitChars, 2);

					string @event = eventSplit[0].ToLower();
					string eventData = eventSplit.Length > 1 ? eventSplit[1] : null; // Handle events with no data

					switch (@event)
					{
						case "round-end-event":
							server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());
							break;

						case "waiting-for-players-event":
							server.IsLoading = false;

							server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers => waitingForPlayers.OnWaitingForPlayers());
							break;

						case "round-start-event":
							server.ForEachHandler<IEventRoundStart>(roundStart => roundStart.OnRoundStart());
							break;

						case "server-start-event":
							server.ForEachHandler<IEventServerStart>(serverStart => serverStart.OnServerStart());
							break;

						case "server-full-event":
							server.ForEachHandler<IEventServerFull>(serverFull => serverFull.OnServerFull());
							break;

						case "set-supported-features":
							if (int.TryParse(eventData, out int supportedFeatures))
							{
								server.supportedModFeatures = (ModFeatures)supportedFeatures;
							}
							break;
					}

					// Don't print any MultiAdmin events
					return;
				}
			}

			server.Write(coloredMessage);
		}
	}
}

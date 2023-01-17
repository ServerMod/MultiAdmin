using System;
using System.Text.RegularExpressions;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public class OutputHandler
	{
		public static readonly Regex SmodRegex =
			new(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
		public static readonly char[] TrimChars = { '.', ' ', '\t', '!', '?', ',' };
		public static readonly char[] EventSplitChars = new char[] { ':' };

		private readonly Server server;

		private enum OutputCodes : byte
		{
			//0x00 - 0x0F - reserved for colors

			RoundRestart = 0x10,
			IdleEnter = 0x11,
			IdleExit = 0x12,
			ExitActionReset = 0x13,
			ExitActionShutdown = 0x14,
			ExitActionSilentShutdown = 0x15,
			ExitActionRestart = 0x16,
			RoundEnd = 0x17
		}

		// Temporary measure to handle round ends until the game updates to use this
		private bool roundEndCodeUsed = false;

		public OutputHandler(Server server)
		{
			this.server = server;
		}

		public void HandleMessage(object? source, ServerSocket.MessageEventArgs message)
		{
			if (message.message == null)
				return;

			ColoredMessage coloredMessage = new(message.message, ConsoleColor.White);

			if (!coloredMessage.text.IsNullOrEmpty())
			{
				// Parse the color byte
				coloredMessage.textColor = (ConsoleColor)message.color;

				// Smod2 loggers pretty printing
				Match match = SmodRegex.Match(coloredMessage.text!);
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

				string lowerMessage = coloredMessage.text!.ToLower();
				if (!server.supportedModFeatures.HasFlag(ModFeatures.CustomEvents))
				{
					switch (lowerMessage.Trim(TrimChars))
					{
						case "the round is about to restart! please wait":
							if (!roundEndCodeUsed)
								server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());
							break;

						/* Replaced by OutputCodes.RoundRestart
						case "waiting for players":
							server.IsLoading = false;
							server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers => waitingForPlayers.OnWaitingForPlayers());
							break;
						*/

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
					string eventMessage = coloredMessage.text[11..];

					// Split event and event data
					string[] eventSplit = eventMessage.Split(EventSplitChars, 2);

					string @event = eventSplit[0].ToLower();
					string? eventData = eventSplit.Length > 1 ? eventSplit[1] : null; // Handle events with no data

					switch (@event)
					{
						case "round-end-event":
							if (!roundEndCodeUsed)
								server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());
							break;

						/* Replaced by OutputCodes.RoundRestart
						case "waiting-for-players-event":
							server.IsLoading = false;
							server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers => waitingForPlayers.OnWaitingForPlayers());
							break;
						*/

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

		public void HandleAction(object? source, byte action)
		{
			switch ((OutputCodes)action)
			{
				// This seems to show up at the waiting for players event
				case OutputCodes.RoundRestart:
					server.IsLoading = false;
					server.ForEachHandler<IEventWaitingForPlayers>(waitingForPlayers => waitingForPlayers.OnWaitingForPlayers());
					break;

				case OutputCodes.IdleEnter:
					server.ForEachHandler<IEventIdleEnter>(idleEnter => idleEnter.OnIdleEnter());
					break;

				case OutputCodes.IdleExit:
					server.ForEachHandler<IEventIdleExit>(idleExit => idleExit.OnIdleExit());
					break;

				// Requests to reset the ExitAction status
				case OutputCodes.ExitActionReset:
					server.SetServerRequestedStatus(ServerStatus.Running);
					break;

				// Requests the Shutdown ExitAction with the intent to restart at any time in the future
				case OutputCodes.ExitActionShutdown:
					server.SetServerRequestedStatus(ServerStatus.ExitActionStop);
					break;

				// Requests the SilentShutdown ExitAction with the intent to restart at any time in the future
				case OutputCodes.ExitActionSilentShutdown:
					server.SetServerRequestedStatus(ServerStatus.ExitActionStop);
					break;

				// Requests the Restart ExitAction status with the intent to restart at any time in the future
				case OutputCodes.ExitActionRestart:
					server.SetServerRequestedStatus(ServerStatus.ExitActionRestart);
					break;

				case OutputCodes.RoundEnd:
					roundEndCodeUsed = true;
					server.ForEachHandler<IEventRoundEnd>(roundEnd => roundEnd.OnRoundEnd());
					break;

				default:
					Program.LogDebug(nameof(HandleAction), $"Received unknown output code ({action}), is MultiAdmin up to date? This error can probably be safely ignored.");
					break;
			}
		}
	}
}

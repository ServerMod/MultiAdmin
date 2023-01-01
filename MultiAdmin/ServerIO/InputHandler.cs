using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public static class InputHandler
	{
		private static readonly char[] Separator = { ' ' };

		public static readonly ColoredMessage BaseSection = new(null, ConsoleColor.White);

		public static readonly ColoredMessage InputPrefix = new("> ", ConsoleColor.Yellow);
		public static readonly ColoredMessage LeftSideIndicator = new("...", ConsoleColor.Yellow);
		public static readonly ColoredMessage RightSideIndicator = new("...", ConsoleColor.Yellow);

		public static int InputPrefixLength => InputPrefix?.Length ?? 0;

		public static int LeftSideIndicatorLength => LeftSideIndicator?.Length ?? 0;
		public static int RightSideIndicatorLength => RightSideIndicator?.Length ?? 0;

		public static int TotalIndicatorLength => LeftSideIndicatorLength + RightSideIndicatorLength;

		public static int SectionBufferWidth
		{
			get
			{
				try
				{
					return Console.BufferWidth - (1 + InputPrefixLength);
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(SectionBufferWidth), e);
					return 0;
				}
			}
		}

		public static string? CurrentMessage { get; private set; }
		public static ColoredMessage?[]? CurrentInput { get; private set; } = { InputPrefix };
		public static int CurrentCursor { get; private set; }

		public static async void Write(Server server, CancellationToken cancellationToken)
		{
			try
			{
				ShiftingList prevMessages = new(25);

				while (server.IsRunning && !server.IsStopping)
				{
					if (Program.Headless)
					{
						break;
					}

					string? message;
					if (server.ServerConfig.ActualConsoleInputSystem == ConsoleInputSystem.New && SectionBufferWidth - TotalIndicatorLength > 0)
					{
						message = await GetInputLineNew(server, prevMessages, cancellationToken);
					}
					else if (server.ServerConfig.ActualConsoleInputSystem == ConsoleInputSystem.Old)
					{
						message = await GetInputLineOld(server, cancellationToken);
					}
					else
					{
						message = Console.ReadLine();
					}

					if (string.IsNullOrEmpty(message)) continue;

					server.Write($">>> {message}", ConsoleColor.DarkMagenta);

					int separatorIndex = message.IndexOfAny(Separator);
					string commandName = (separatorIndex < 0 ? message : message[..separatorIndex]).ToLower().Trim();
					if (commandName.IsNullOrEmpty()) continue;

					bool callServer = true;
					if (server.commands.TryGetValue(commandName, out ICommand? command))
					{
						try
						{
							// Use double quotation marks to escape a quotation mark
							command.OnCall(separatorIndex < 0 || separatorIndex + 1 >= message.Length ? Array.Empty<string>() : CommandUtils.StringToArgs(message, separatorIndex + 1, escapeChar: '\"', quoteChar: '\"'));
						}
						catch (Exception e)
						{
							server.Write($"Error in command \"{commandName}\":{Environment.NewLine}{e}");
						}

						callServer = command.PassToGame();
					}

					if (callServer) server.SendMessage(message);
				}

				ResetInputParams();
			}
			catch (TaskCanceledException)
			{
				// Exit the Task immediately if cancelled
			}
		}

		/// <summary>
		/// Waits until <see cref="Console.KeyAvailable"/> returns true.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to check for cancellation.</param>
		/// <exception cref="TaskCanceledException">The task has been canceled.</exception>
		public static async Task WaitForKey(CancellationToken cancellationToken)
		{
			while (!Console.KeyAvailable)
			{
				await Task.Delay(10, cancellationToken);
			}
		}

		public static async Task<string> GetInputLineOld(Server server, CancellationToken cancellationToken)
		{
			StringBuilder message = new();
			while (true)
			{
				await WaitForKey(cancellationToken);

				ConsoleKeyInfo key = Console.ReadKey(server.ServerConfig.HideInput.Value);

				switch (key.Key)
				{
					case ConsoleKey.Backspace:
						if (!message.IsEmpty())
							message.Remove(message.Length - 1, 1);
						break;

					case ConsoleKey.Enter:
						return message.ToString();

					default:
						message.Append(key.KeyChar);
						break;
				}
			}
		}

		public static async Task<string> GetInputLineNew(Server server, ShiftingList prevMessages, CancellationToken cancellationToken)
		{
			if (server.ServerConfig.RandomInputColors.Value)
				RandomizeInputColors();

			string curMessage = "";
			string message = "";
			int messageCursor = 0;
			int prevMessageCursor = -1;
			StringSections? curSections = null;
			int lastSectionIndex = -1;
			bool exitLoop = false;
			while (!exitLoop)
			{
				#region Key Press Handling

				await WaitForKey(cancellationToken);

				ConsoleKeyInfo key = Console.ReadKey(true);

				switch (key.Key)
				{
					case ConsoleKey.Backspace:
						if (messageCursor > 0 && !message.IsEmpty())
							message = message.Remove(--messageCursor, 1);

						break;

					case ConsoleKey.Delete:
						if (messageCursor >= 0 && messageCursor < message.Length)
							message = message.Remove(messageCursor, 1);

						break;

					case ConsoleKey.Enter:
						exitLoop = true;
						break;

					case ConsoleKey.UpArrow:
						prevMessageCursor++;
						if (prevMessageCursor >= prevMessages.Count)
							prevMessageCursor = prevMessages.Count - 1;

						message = prevMessageCursor < 0 ? curMessage : prevMessages[prevMessageCursor];

						break;

					case ConsoleKey.DownArrow:
						prevMessageCursor--;
						if (prevMessageCursor < -1)
							prevMessageCursor = -1;

						message = prevMessageCursor < 0 ? curMessage : prevMessages[prevMessageCursor];

						break;

					case ConsoleKey.LeftArrow:
						messageCursor--;
						break;

					case ConsoleKey.RightArrow:
						messageCursor++;
						break;

					case ConsoleKey.Home:
						messageCursor = 0;
						break;

					case ConsoleKey.End:
						messageCursor = message.Length;
						break;

					case ConsoleKey.PageUp:
						messageCursor -= SectionBufferWidth - TotalIndicatorLength;
						break;

					case ConsoleKey.PageDown:
						messageCursor += SectionBufferWidth - TotalIndicatorLength;
						break;

					default:
						message = message.Insert(messageCursor++, key.KeyChar.ToString());
						break;
				}

				#endregion

				if (prevMessageCursor < 0)
					curMessage = message;

				// If the input is done and should exit the loop, break from the while loop
				if (exitLoop)
					break;

				if (messageCursor < 0)
					messageCursor = 0;
				else if (messageCursor > message.Length)
					messageCursor = message.Length;

				#region Input Printing Management

				// If the message has changed, re-write it to the console
				if (CurrentMessage != message)
				{
					if (message.Length > SectionBufferWidth && SectionBufferWidth - TotalIndicatorLength > 0)
					{
						curSections = GetStringSections(message);

						StringSection? curSection =
							curSections.GetSection(IndexMinusOne(messageCursor), out int sectionIndex);

						if (curSection != null)
						{
							lastSectionIndex = sectionIndex;

							SetCurrentInput(curSection.Value.Section);
							CurrentCursor = curSection.Value.GetRelativeIndex(messageCursor);
							WriteInputAndSetCursor(true);
						}
						else
						{
							server.Write("Error while processing input string: curSection is null!", ConsoleColor.Red);
						}
					}
					else
					{
						curSections = null;

						SetCurrentInput(message);
						CurrentCursor = messageCursor;

						WriteInputAndSetCursor(true);
					}
				}
				else if (CurrentCursor != messageCursor)
				{
					try
					{
						// If the message length is longer than the buffer width (being cut into sections), re-write the message
						if (curSections != null)
						{
							StringSection? curSection =
								curSections.GetSection(IndexMinusOne(messageCursor), out int sectionIndex);

							if (curSection != null)
							{
								CurrentCursor = curSection.Value.GetRelativeIndex(messageCursor);

								// If the cursor index is in a different section from the last section, fully re-draw it
								if (lastSectionIndex != sectionIndex)
								{
									lastSectionIndex = sectionIndex;

									SetCurrentInput(curSection.Value.Section);

									WriteInputAndSetCursor(true);
								}

								// Otherwise, if only the relative cursor index has changed, set only the cursor
								else
								{
									SetCursor();
								}
							}
							else
							{
								server.Write("Error while processing input string: curSection is null!",
									ConsoleColor.Red);
							}
						}
						else
						{
							CurrentCursor = messageCursor;
							SetCursor();
						}
					}
					catch (Exception e)
					{
						Program.LogDebugException(nameof(Write), e);

						CurrentCursor = messageCursor;
						SetCursor();
					}
				}

				CurrentMessage = message;

				#endregion
			}

			// Reset the current input parameters
			ResetInputParams();

			if (!string.IsNullOrEmpty(message))
				prevMessages.Add(message);

			return message;
		}

		public static void ResetInputParams()
		{
			CurrentMessage = null;
			SetCurrentInput();
			CurrentCursor = 0;
		}

		public static void SetCurrentInput(params ColoredMessage?[]? coloredMessages)
		{
			List<ColoredMessage?> message = new() { InputPrefix };

			if (coloredMessages != null)
				message.AddRange(coloredMessages);

			CurrentInput = message.ToArray();
		}

		public static void SetCurrentInput(string message)
		{
			ColoredMessage? baseSection = BaseSection?.Clone();

			if (baseSection == null)
				baseSection = new ColoredMessage(message);
			else
				baseSection.text = message;

			SetCurrentInput(baseSection);
		}

		private static StringSections GetStringSections(string message)
		{
			return StringSections.FromString(message, SectionBufferWidth, LeftSideIndicator, RightSideIndicator,
				BaseSection);
		}

		private static int IndexMinusOne(int index)
		{
			// Get the current section that the cursor is in (-1 so that the text before the cursor is displayed at an indicator)
			return Math.Max(index - 1, 0);
		}

		#region Console Management Methods

		public static void SetCursor(int messageCursor)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Program.Headless) return;

				try
				{
					Console.CursorLeft = messageCursor + InputPrefixLength;
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(SetCursor), e);
				}
			}
		}

		public static void SetCursor()
		{
			SetCursor(CurrentCursor);
		}

		public static void WriteInput(ColoredMessage?[]? message, bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Program.Headless) return;

				message?.Write(clearConsoleLine);

				CurrentInput = message;
			}
		}

		public static void WriteInput(bool clearConsoleLine = false)
		{
			WriteInput(CurrentInput, clearConsoleLine);
		}

		public static void WriteInputAndSetCursor(bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				WriteInput(clearConsoleLine);
				SetCursor();
			}
		}

		#endregion

		public static void RandomizeInputColors()
		{
			try
			{
				Random random = new();
				ConsoleColor[] colors = Enum.GetValues<ConsoleColor>();

				ConsoleColor random1 = colors[random.Next(colors.Length)];
				ConsoleColor random2 = colors[random.Next(colors.Length)];

				BaseSection.textColor = random1;

				InputPrefix.textColor = random2;
				LeftSideIndicator.textColor = random2;
				RightSideIndicator.textColor = random2;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(RandomizeInputColors), e);
			}
		}

		public enum ConsoleInputSystem
		{
			// Represents the default input system, which calls Console.ReadLine and blocks the calling context
			Original,
			// Represents the "old" input system, which calls non-blocking methods
			Old,
			// Represents the "new" input system, which also calls non-blocking methods,
			// but the main difference is great display
			New,
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MultiAdmin.Config;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public static class InputHandler
	{
		private static readonly char[] Separator = {' '};

		public static readonly ColoredMessage BaseSection = new ColoredMessage(null, ConsoleColor.White);

		public static readonly ColoredMessage InputPrefix = new ColoredMessage("> ", ConsoleColor.Yellow);
		public static readonly ColoredMessage LeftSideIndicator = new ColoredMessage("...", ConsoleColor.Yellow);
		public static readonly ColoredMessage RightSideIndicator = new ColoredMessage("...", ConsoleColor.Yellow);

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

		public static string CurrentMessage { get; private set; }
		public static ColoredMessage[] CurrentInput { get; private set; } = {InputPrefix};
		public static int CurrentCursor { get; private set; }

		public static void Write(Server server)
		{
			try
			{
				ShiftingList prevMessages = new ShiftingList(25);

				while (server.IsRunning && !server.IsStopping)
				{
					if (Program.Headless)
					{
						Thread.Sleep(5000);
						continue;
					}

					string message = server.ServerConfig.UseNewInputSystem.Value ? GetInputLineNew(server, prevMessages) : Console.ReadLine();

					if (string.IsNullOrEmpty(message)) continue;

					server.Write($">>> {message}", ConsoleColor.DarkMagenta);

					string[] messageSplit = message.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
					if (messageSplit.IsEmpty()) continue;

					bool callServer = true;
					server.commands.TryGetValue(messageSplit[0].ToLower().Trim(), out ICommand command);
					if (command != null)
					{
						command.OnCall(messageSplit.Skip(1).Take(messageSplit.Length - 1).ToArray());
						callServer = command.PassToGame();
					}

					if (callServer) server.SendMessage(message);
				}

				ResetInputParams();
			}
			catch (ThreadInterruptedException)
			{
				// Exit the Thread immediately if interrupted
			}
		}

		public static string GetInputLineNew(Server server, ShiftingList prevMessages)
		{
			if (server.ServerConfig.RandomInputColors.Value)
				RandomizeInputColors();

			string curMessage = string.Empty;
			string message = string.Empty;
			int messageCursor = 0;
			int prevMessageCursor = -1;
			StringSections curSections = null;
			int lastSectionIndex = -1;
			bool exitLoop = false;
			while (!exitLoop)
			{
				#region Key Press Handling

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

				// If the input is done and should exit the loop, this will cause the loop to be exited and the input to be processed
				if (exitLoop)
				{
					// Reset the current input parameters
					ResetInputParams();

					if (!string.IsNullOrEmpty(message))
						prevMessages.Add(message);

					return message;
				}

				if (messageCursor < 0)
					messageCursor = 0;
				else if (messageCursor > message.Length)
					messageCursor = message.Length;

				#region Input Printing Management

				// If the message has changed, re-write it to the console
				if (CurrentMessage != message)
				{
					if (message.Length > SectionBufferWidth)
					{
						curSections = GetStringSections(message);

						StringSection? curSection = curSections.GetSection(IndexMinusOne(messageCursor), out int sectionIndex);

						if (curSection != null)
						{
							lastSectionIndex = sectionIndex;

							SetCurrentInput(curSection.Value.Section);
							CurrentCursor = curSection.Value.GetRelativeIndex(messageCursor);

							WriteInputAndSetCursor();
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

						WriteInputAndSetCursor();
					}
				}
				else if (CurrentCursor != messageCursor)
				{
					try
					{
						// If the message length is longer than the buffer width (being cut into sections), re-write the message
						if (curSections != null)
						{
							StringSection? curSection = curSections.GetSection(IndexMinusOne(messageCursor), out int sectionIndex);

							if (curSection != null)
							{
								CurrentCursor = curSection.Value.GetRelativeIndex(messageCursor);

								// If the cursor index is in a different section from the last section, fully re-draw it
								if (lastSectionIndex != sectionIndex)
								{
									lastSectionIndex = sectionIndex;

									SetCurrentInput(curSection.Value.Section);

									WriteInputAndSetCursor();
								}

								// Otherwise, if only the relative cursor index has changed, set only the cursor
								else
								{
									SetCursor();
								}
							}
							else
							{
								server.Write("Error while processing input string: curSection is null!", ConsoleColor.Red);
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

			return null;
		}

		public static void ResetInputParams()
		{
			CurrentMessage = null;
			SetCurrentInput();
			CurrentCursor = 0;
		}

		public static void SetCurrentInput(params ColoredMessage[] coloredMessages)
		{
			List<ColoredMessage> message = new List<ColoredMessage> {InputPrefix};

			if (coloredMessages != null)
				message.AddRange(coloredMessages);

			CurrentInput = message.ToArray();
		}

		public static void SetCurrentInput(string message)
		{
			ColoredMessage baseSection = BaseSection?.Clone();

			if (baseSection == null)
				baseSection = new ColoredMessage(message);
			else
				baseSection.text = message;

			SetCurrentInput(baseSection);
		}

		private static StringSections GetStringSections(string message)
		{
			return StringSections.FromString(message, SectionBufferWidth, LeftSideIndicator, RightSideIndicator, BaseSection);
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

		public static void WriteInput(ColoredMessage[] message)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Program.Headless) return;

				message?.Write(MultiAdminConfig.GlobalConfig.UseNewInputSystem.Value);

				CurrentInput = message;
			}
		}

		public static void WriteInput()
		{
			WriteInput(CurrentInput);
		}

		public static void WriteInputAndSetCursor()
		{
			lock (ColoredConsole.WriteLock)
			{
				WriteInput();
				SetCursor();
			}
		}

		#endregion

		public static void RandomizeInputColors()
		{
			try
			{
				Random random = new Random();
				Array colors = Enum.GetValues(typeof(ConsoleColor));

				ConsoleColor random1 = (ConsoleColor)colors.GetValue(random.Next(colors.Length));
				ConsoleColor random2 = (ConsoleColor)colors.GetValue(random.Next(colors.Length));

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
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MultiAdmin
{
	public static class InputThread
	{
		private static readonly char[] Separator = {' '};

		public static readonly ColoredMessage InputPrefix = new ColoredMessage("> ", ConsoleColor.DarkMagenta);
		public static readonly ColoredMessage LeftSideIndicator = new ColoredMessage("...", ConsoleColor.DarkMagenta);
		public static readonly ColoredMessage RightSideIndicator = new ColoredMessage("...", ConsoleColor.DarkMagenta);

		public static int InputPrefixLength => InputPrefix?.Length ?? 0;
		public static int LeftSideIndicatorLength => LeftSideIndicator?.Length ?? 0;
		public static int RightSideIndicatorLength => RightSideIndicator?.Length ?? 0;

		public static int SectionBufferWidth
		{
			get
			{
				try
				{
					return Console.BufferWidth - (1 + InputPrefixLength);
				}
				catch
				{
					return 0;
				}
			}
		}

		public static string CurrentInput { get; private set; }
		public static int CurrentCursor { get; private set; }

		public static void Write(Server server)
		{
			while (server.IsRunning && !server.IsStopping)
			{
				if (Program.Headless)
				{
					Thread.Sleep(5000);
					continue;
				}

				string message = string.Empty;
				int messageCursor = 0;
				bool exitLoop = false;
				while (!exitLoop)
				{
					ConsoleKeyInfo key = Console.ReadKey(true);

					switch (key.Key)
					{
						case ConsoleKey.Backspace:
							if (message.Length > 0)
							{
								message = SubText(message, messageCursor--);
							}

							break;

						case ConsoleKey.Delete:
							if (message.Length > 0 && messageCursor < message.Length)
							{
								message = SubText(message, messageCursor + 1);
							}

							break;

						case ConsoleKey.Enter:
							exitLoop = true;
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
							messageCursor += SectionBufferWidth - (LeftSideIndicatorLength + RightSideIndicatorLength);
							break;

						case ConsoleKey.PageDown:
							messageCursor -= SectionBufferWidth - (LeftSideIndicatorLength + RightSideIndicatorLength);
							break;

						default:
							if (key.Key == ConsoleKey.V && (key.Modifiers & ConsoleModifiers.Control) != 0)
							{
								try
								{
									message = AddText(message, Clipboard.GetText(), messageCursor++);
								}
								catch
								{
									// ignored
								}
							}
							else
							{
								message = AddText(message, key.KeyChar.ToString(), messageCursor++);
							}

							break;
					}

					// If the input is done and should exit the loop, this will cause the loop to be exited and the input to be processed
					if (exitLoop)
					{
						// Reset the current input parameters
						CurrentInput = string.Empty;
						CurrentCursor = 0;

						break;
					}

					if (messageCursor < 0)
						messageCursor = 0;
					else if (messageCursor > message.Length)
						messageCursor = message.Length;

					// If the message has changed, re-write it to the console
					if (CurrentInput != message)
					{
						WriteInput(message, messageCursor);
					}
					else if (CurrentCursor != messageCursor)
					{
						try
						{
							// If the message length is longer than the buffer width (being cut into sections), re-write the message
							if (message.Length > Console.BufferWidth - (1 + InputPrefixLength))
								WriteInput(message, messageCursor);

							// Otherwise only set the cursor position
							else
								SetCursor(messageCursor);
						}
						catch
						{
							SetCursor(messageCursor);
						}
					}

					CurrentInput = message;
					CurrentCursor = messageCursor;
				}

				server.Write($">>> {message}", ConsoleColor.DarkMagenta);

				string[] messageSplit = message.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
				if (messageSplit.Length == 0) continue;

				bool callServer = true;
				server.commands.TryGetValue(messageSplit[0].ToLower().Trim(), out ICommand command);
				if (command != null)
				{
					command.OnCall(messageSplit.Skip(1).Take(messageSplit.Length - 1).ToArray());
					callServer = command.PassToGame();
				}

				if (callServer) server.SendMessage(message);
			}

			CurrentInput = null;
			CurrentCursor = 0;
		}

		private static string AddText(string origString, string textToAdd, int index = 0)
		{
			if (origString == null || textToAdd == null) return null;

			if (origString.Length <= 0)
				return textToAdd;

			if (textToAdd.Length <= 0)
				return origString;

			bool atEnd = index >= origString.Length;
			bool atStart = index <= 0;

			return (atStart ? string.Empty : atEnd ? origString : origString.Remove(index)) + textToAdd + (atEnd ? string.Empty : atStart ? origString : origString.Substring(index));
		}

		private static string SubText(string origString, int index = 1, int count = 1)
		{
			if (origString == null) return null;

			return index <= 0 ? origString : origString.Remove(index >= origString.Length ? origString.Length - 1 : index - 1, count);
		}

		private static void SetCursor(int messageCursor)
		{
			try
			{
				Console.CursorLeft = messageCursor + InputPrefixLength;
			}
			catch
			{
				// ignored
			}
		}

		public static void WriteInput(string input, int messageCursor)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Program.Headless) return;

				List<ColoredMessage> output = new List<ColoredMessage> {InputPrefix};

				bool displayTextSet = false;
				try
				{
					if (input.Length > SectionBufferWidth)
					{
						// Split the string into sections with side indicators
						StringSections stringSections = GetStringSections(input, SectionBufferWidth, LeftSideIndicator, RightSideIndicator);

						// Get the current section that the cursor is in (-1 so that the text before the cursor is displayed at an indicator)
						if (stringSections.GetSection(messageCursor <= 0 ? 0 : messageCursor - 1) is StringSection section)
						{
							// Set the displayed input text to the section text
							output.AddRange(section.Section);
							displayTextSet = true;
							// Get the relative point in the console that the cursor would be at based on the string index
							messageCursor = section.GetRelativeIndex(messageCursor);
						}
					}
				}
				catch
				{
					// ignored
				}

				if (!displayTextSet)
					output.Add(new ColoredMessage(input, ConsoleColor.Magenta));

				Program.ClearConsoleLine();
				output.Write();

				SetCursor(messageCursor);
			}
		}

		public static void WriteInput()
		{
			WriteInput(CurrentInput, CurrentCursor);
		}

		private static StringSections GetStringSections(string fullString, int sectionLength, ColoredMessage leftIndicator, ColoredMessage rightIndicator, ConsoleColor sectionColor = ConsoleColor.Magenta)
		{
			List<StringSection> sections = new List<StringSection>();

			// The starting index of the current section being created
			int sectionStartIndex = 0;

			// The text of the current section being created
			string curSecString = string.Empty;

			for (int i = 0; i < fullString.Length; i++)
			{
				curSecString += fullString[i];

				// If the section is less than the smallest possible section size, skip processing
				if (curSecString.Length < sectionLength - ((leftIndicator?.Length ?? 0) + (rightIndicator?.Length ?? 0))) continue;

				// Decide what the left indicator text should be accounting for the leftmost section
				ColoredMessage leftIndicatorSection = sections.Count > 0 ? leftIndicator : null;
				// Decide what the right indicator text should be accounting for the rightmost section
				ColoredMessage rightIndicatorSection = i < fullString.Length - (1 + (rightIndicator?.Length ?? 0)) ? rightIndicator : null;

				// Check the section length against the final section length
				if (curSecString.Length >= sectionLength - ((leftIndicatorSection?.Length ?? 0) + (rightIndicatorSection?.Length ?? 0)))
				{
					// Instantiate the section with the final parameters
					sections.Add(new StringSection(new ColoredMessage(curSecString, sectionColor), leftIndicatorSection, rightIndicatorSection, sectionStartIndex, i));

					// Reset the current section being worked on
					curSecString = string.Empty;
					sectionStartIndex = i + 1;
				}
			}

			// If there's still text remaining in a section that hasn't been processed, add it as a section
			if (curSecString.Length > 0)
			{
				// Only decide for the left indicator, as this last section will always be the rightmost section
				ColoredMessage leftIndicatorSection = sections.Count > 0 ? leftIndicator : null;

				sections.Add(new StringSection(new ColoredMessage(curSecString, sectionColor), leftIndicatorSection, null, sectionStartIndex, fullString.Length));
			}

			return new StringSections(sections.ToArray());
		}

		private struct StringSections
		{
			public StringSection[] Sections { get; }

			public StringSections(StringSection[] sections)
			{
				Sections = sections;
			}

			public StringSection? GetSection(int index)
			{
				foreach (StringSection stringSection in Sections)
				{
					if (stringSection.IsWithinSection(index))
						return stringSection;
				}

				return null;
			}
		}

		private struct StringSection
		{
			public ColoredMessage Text { get; }

			public ColoredMessage LeftIndicator { get; }
			public ColoredMessage RightIndicator { get; }

			public ColoredMessage[] Section => new ColoredMessage[] {LeftIndicator, Text, RightIndicator};

			public int MinIndex { get; }
			public int MaxIndex { get; }

			public StringSection(ColoredMessage text, ColoredMessage leftIndicator, ColoredMessage rightIndicator, int minIndex, int maxIndex)
			{
				Text = text;

				LeftIndicator = leftIndicator;
				RightIndicator = rightIndicator;

				MinIndex = minIndex;
				MaxIndex = maxIndex;
			}

			public bool IsWithinSection(int index)
			{
				return index >= MinIndex && index <= MaxIndex;
			}

			public int GetRelativeIndex(int index)
			{
				return index - MinIndex + (LeftIndicator?.Length ?? 0);
			}
		}
	}
}
using System;

namespace MultiAdmin.ConsoleTools
{
	public static class ConsoleUtils
	{
		#region Clear Console Line Methods

		private static bool IsIndexWithinBuffer(int index)
		{
			return index >= 0 && index < Console.BufferWidth;
		}

		public static void ClearConsoleLine(int index, bool returnCursorPos = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Program.Headless) return;

				try
				{
					int cursorLeftReturnIndex = returnCursorPos ? Console.CursorLeft : 0;
					// Linux console uses visible section as a scrolling buffer,
					// that means that making the console taller moves CursorTop to a higher index,
					// but when the user makes the console smaller, CursorTop is left at a higher index than BufferHeight,
					// causing an error
					int cursorTopIndex = Math.Min(Console.CursorTop, Console.BufferHeight - 1);

					Console.SetCursorPosition(IsIndexWithinBuffer(index) ? index : 0, cursorTopIndex);

					// If the message stretches to the end of the console window, the console window will generally wrap the line into a new line,
					// so 1 is subtracted
					int charCount = Console.BufferWidth - Console.CursorLeft - 1;
					if (charCount > 0)
					{
						Console.Write(new string(' ', charCount));
					}

					Console.SetCursorPosition(IsIndexWithinBuffer(cursorLeftReturnIndex) ? cursorLeftReturnIndex : 0, cursorTopIndex);
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(ClearConsoleLine), e);
				}
			}
		}

		public static string? ClearConsoleLine(string? message)
		{
			if (!string.IsNullOrEmpty(message))
				ClearConsoleLine(message.Contains(Environment.NewLine) ? 0 : message.Length);
			else
				ClearConsoleLine(0);

			return message;
		}

		public static ColoredMessage? ClearConsoleLine(ColoredMessage? message)
		{
			ClearConsoleLine(message?.text);
			return message;
		}

		public static ColoredMessage?[] ClearConsoleLine(ColoredMessage?[] message)
		{
			ClearConsoleLine(message?.GetText());
			return message!;
		}

		#endregion
	}
}

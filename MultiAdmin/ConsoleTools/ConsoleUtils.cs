using System;
using System.Collections.Generic;

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
					int lastCursor = returnCursorPos ? Console.CursorLeft : 0;
					int newCursor = IsIndexWithinBuffer(index) ? index : 0;

					if (newCursor != lastCursor)
						Console.CursorLeft = newCursor;

					int charCount = Console.BufferWidth - Console.CursorLeft - 1;
					if (charCount > 0)
						Console.Write(new string(' ', charCount));

					if (newCursor != lastCursor)
						Console.CursorLeft = lastCursor;
				}
				catch (Exception e)
				{
					Program.LogDebugException("ClearConsoleLine", e);
				}
			}
		}

		public static string ClearConsoleLine(string message)
		{
			if (!string.IsNullOrEmpty(message))
				ClearConsoleLine(message.Contains(Environment.NewLine) ? 0 : message.Length);
			else
				ClearConsoleLine(0);

			return message;
		}

		public static ColoredMessage ClearConsoleLine(ColoredMessage message)
		{
			ClearConsoleLine(message?.text);
			return message;
		}

		public static ColoredMessage[] ClearConsoleLine(ColoredMessage[] message)
		{
			ClearConsoleLine(message?.GetText());
			return message;
		}

		public static List<ColoredMessage> ClearConsoleLine(List<ColoredMessage> message)
		{
			ClearConsoleLine(message?.GetText());
			return message;
		}

		#endregion
	}
}

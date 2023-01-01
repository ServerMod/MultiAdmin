using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAdmin.Utility
{
	public static class CommandUtils
	{
		public static int IndexOfNonEscaped(string inString, char inChar, int startIndex, int count, char escapeChar = '\\')
		{
			if (inString == null)
			{
				throw new NullReferenceException();
			}

			if (startIndex < 0 || startIndex >= inString.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (count < 0 || startIndex + count > inString.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			bool escaped = false;
			for (int i = 0; i < count; i++)
			{
				int stringIndex = startIndex + i;
				char stringChar = inString[stringIndex];

				if (!escaped)
				{
					if (stringChar == escapeChar && (escapeChar != inChar || ((i + 1) < count && inString[startIndex + i + 1] == escapeChar)))
					{
						escaped = true;
						continue;
					}
				}

				// If the character isn't escaped or the character that's escaped is an escape character then check if it matches
				if ((!escaped || (stringChar == escapeChar && escapeChar != inChar)) && stringChar == inChar)
				{
					return stringIndex;
				}

				escaped = false;
			}

			return -1;
		}

		public static int IndexOfNonEscaped(string inString, char inChar, int startIndex, char escapeChar = '\\')
		{
			return IndexOfNonEscaped(inString, inChar, startIndex, inString.Length - startIndex, escapeChar);
		}

		public static int IndexOfNonEscaped(string inString, char inChar, char escapeChar = '\\')
		{
			return IndexOfNonEscaped(inString, inChar, 0, inString.Length, escapeChar);
		}

		public static string[] StringToArgs(string inString, int startIndex, int count, char separator = ' ', char escapeChar = '\\', char quoteChar = '\"', bool keepQuotes = false)
		{
			if (startIndex < 0 || startIndex >= inString.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (count < 0 || startIndex + count > inString.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (inString.IsEmpty())
				return Array.Empty<string>();

			List<string> args = new();
			StringBuilder strBuilder = new();
			bool inQuotes = false;
			bool escaped = false;

			for (int i = 0; i < count; i++)
			{
				char stringChar = inString[startIndex + i];

				if (!escaped)
				{
					if (stringChar == escapeChar && (escapeChar != quoteChar || ((i + 1) < count && inString[startIndex + i + 1] == escapeChar)))
					{
						escaped = true;
						continue;
					}

					if (stringChar == quoteChar && (inQuotes || ((i + 1) < count && IndexOfNonEscaped(inString, quoteChar, startIndex + (i + 1), count - (i + 1), escapeChar) > 0)))
					{
						// Ignore quotes if there's no future non-escaped quotes

						inQuotes = !inQuotes;
						if (!keepQuotes)
							continue;
					}
					else if (!inQuotes && stringChar == separator)
					{
						args.Add(strBuilder.ToString());
						strBuilder.Clear();
						continue;
					}
				}

				strBuilder.Append(stringChar);
				escaped = false;
			}

			args.Add(strBuilder.ToString());

			return args.ToArray();
		}

		public static string[] StringToArgs(string inString, int startIndex, char separator = ' ', char escapeChar = '\\', char quoteChar = '\"', bool keepQuotes = false)
		{
			return StringToArgs(inString, startIndex, inString.Length - startIndex, separator, escapeChar, quoteChar, keepQuotes);
		}

		public static string[] StringToArgs(string inString, char separator = ' ', char escapeChar = '\\', char quoteChar = '\"', bool keepQuotes = false)
		{
			return StringToArgs(inString, 0, inString.Length, separator, escapeChar, quoteChar, keepQuotes);
		}
	}
}

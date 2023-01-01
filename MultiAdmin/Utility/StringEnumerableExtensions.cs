using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAdmin.Utility
{
	public static class StringEnumerableExtensions
	{
		public static string JoinArgs(this IEnumerable<string?> args)
		{
			StringBuilder argsStringBuilder = new();
			foreach (string? arg in args)
			{
				if (string.IsNullOrEmpty(arg))
					continue;

				// Escape escape characters (if not on Windows) and quotation marks
				string escapedArg = OperatingSystem.IsWindows() ? arg.Replace("\"", "\\\"") : arg.Replace("\\", "\\\\").Replace("\"", "\\\"");

				// Separate with spaces
				if (!argsStringBuilder.IsEmpty())
					argsStringBuilder.Append(' ');

				// Handle spaces by surrounding with quotes
				if (escapedArg.Contains(' '))
				{
					argsStringBuilder.Append('"');
					argsStringBuilder.Append(escapedArg);
					argsStringBuilder.Append('"');
				}
				else
				{
					argsStringBuilder.Append(escapedArg);
				}
			}

			return argsStringBuilder.ToString();
		}
	}
}

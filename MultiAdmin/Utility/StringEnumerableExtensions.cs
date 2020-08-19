using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiAdmin.Utility
{
	public static class StringEnumerableExtensions
	{
		public static string JoinArgs(this IEnumerable<string> args)
		{
			StringBuilder argsStringBuilder = new StringBuilder();
			foreach (string arg in args)
			{
				if (arg.IsNullOrEmpty())
					continue;

				// Separate with spaces
				if (!argsStringBuilder.IsEmpty())
					argsStringBuilder.Append(' ');

				// Handle spaces by surrounding with quotes
				if (arg.Contains(' '))
				{
					argsStringBuilder.Append('"');
					argsStringBuilder.Append(arg);
					argsStringBuilder.Append('"');
				}
				else
				{
					argsStringBuilder.Append(arg);
				}
			}

			return argsStringBuilder.ToString();
		}
	}
}

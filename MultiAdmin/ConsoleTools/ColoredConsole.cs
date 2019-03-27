using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiAdmin.ConsoleTools
{
	public class ColoredConsole
	{
		public static readonly object WriteLock = new object();

		public static void Write(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
		{
			lock (WriteLock)
			{
				if (text == null) return;

				ConsoleColor lastFore = Console.ForegroundColor;
				ConsoleColor lastBack = Console.BackgroundColor;

				Console.ForegroundColor = textColor;
				Console.BackgroundColor = backgroundColor;

				Console.Write(text);

				Console.ForegroundColor = lastFore;
				Console.BackgroundColor = lastBack;
			}
		}

		public static void WriteLine(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
		{
			lock (WriteLock)
			{
				Write(text, textColor, backgroundColor);

				Console.WriteLine();
			}
		}

		public static void Write(params ColoredMessage[] message)
		{
			lock (WriteLock)
			{
				foreach (ColoredMessage coloredMessage in message)
				{
					if (coloredMessage != null)
						Write(coloredMessage.text, coloredMessage.textColor, coloredMessage.backgroundColor);
				}
			}
		}

		public static void WriteLine(params ColoredMessage[] message)
		{
			lock (WriteLock)
			{
				Write(message);

				Console.WriteLine();
			}
		}

		public static void WriteLines(params ColoredMessage[] message)
		{
			lock (WriteLock)
			{
				foreach (ColoredMessage coloredMessage in message) WriteLine(coloredMessage);
			}
		}
	}

	public class ColoredMessage : ICloneable
	{
		public string text;
		public ConsoleColor textColor;
		public ConsoleColor backgroundColor;

		public int Length => text?.Length ?? 0;

		public ColoredMessage(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
		{
			this.text = text;
			this.textColor = textColor;
			this.backgroundColor = backgroundColor;
		}

		public override string ToString()
		{
			return text;
		}

		public ColoredMessage Clone()
		{
			return new ColoredMessage(text?.Clone() as string, textColor, backgroundColor);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Write(bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				ColoredConsole.Write(clearConsoleLine ? ConsoleUtils.ClearConsoleLine(this) : this);
			}
		}

		public void WriteLine(bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				ColoredConsole.WriteLine(clearConsoleLine ? ConsoleUtils.ClearConsoleLine(this) : this);
			}
		}
	}

	public static class ColoredMessageEnumerableExtensions
	{
		private static string JoinTextIgnoreNull(IEnumerable<object> objects)
		{
			StringBuilder builder = new StringBuilder(string.Empty);

			foreach (object o in objects)
			{
				if (o != null)
					builder.Append(o);
			}

			return builder.ToString();
		}

		public static string GetText(this IEnumerable<ColoredMessage> message)
		{
			return JoinTextIgnoreNull(message);
		}

		public static void Write(this IEnumerable<ColoredMessage> message, bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				ColoredConsole.Write(clearConsoleLine ? ConsoleUtils.ClearConsoleLine(message.ToArray()) : message.ToArray());
			}
		}

		public static void WriteLine(this IEnumerable<ColoredMessage> message, bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				ColoredConsole.WriteLine(clearConsoleLine ? ConsoleUtils.ClearConsoleLine(message.ToArray()) : message.ToArray());
			}
		}

		public static void WriteLines(this IEnumerable<ColoredMessage> message, bool clearConsoleLine = false)
		{
			lock (ColoredConsole.WriteLock)
			{
				ColoredConsole.WriteLines(clearConsoleLine ? ConsoleUtils.ClearConsoleLine(message.ToArray()) : message.ToArray());
			}
		}
	}
}

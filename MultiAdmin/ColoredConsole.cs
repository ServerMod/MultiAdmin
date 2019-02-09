using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiAdmin
{
	public class ColoredConsole
	{
		public static readonly object WriteLock = new object();

		public static void Write(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
		{
			lock (WriteLock)
			{
				if (text == null) return;

				Console.ForegroundColor = textColor;
				Console.BackgroundColor = backgroundColor;

				Console.Write(text);

				Console.ForegroundColor = ConsoleColor.White;
				Console.BackgroundColor = ConsoleColor.Black;
			}
		}

		public static void WriteLine(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
		{
			lock (WriteLock)
			{
				if (text != null)
					Write(text, textColor, backgroundColor);

				Console.WriteLine();
			}
		}

		public static void Write(params ColoredMessage[] message)
		{
			lock (WriteLock)
			{
				foreach (ColoredMessage coloredMessage in message) Write(coloredMessage.text, coloredMessage.textColor, coloredMessage.backgroundColor);
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
			return new ColoredMessage(text.Clone() as string, textColor, backgroundColor);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Write()
		{
			ColoredConsole.Write(this);
		}

		public void WriteLine()
		{
			ColoredConsole.WriteLine(this);
		}
	}

	public static class ColoredMessageEnumerableExtensions
	{
		public static string GetText(this IEnumerable<ColoredMessage> message)
		{
			return string.Join("", message);
		}

		public static void Write(this IEnumerable<ColoredMessage> message)
		{
			ColoredConsole.Write(message.ToArray());
		}

		public static void WriteLine(this IEnumerable<ColoredMessage> message)
		{
			ColoredConsole.WriteLine(message.ToArray());
		}

		public static void WriteLines(this IEnumerable<ColoredMessage> message)
		{
			ColoredConsole.WriteLines(message.ToArray());
		}
	}
}
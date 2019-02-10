using System;
using System.IO;

namespace MultiAdmin
{
	public static class Utils
	{
		public static string DateTime => System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm");

		public static string TimeStamp
		{
			get
			{
				DateTime now = System.DateTime.Now;
				return $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}]";
			}
		}

		public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;
		public static bool IsMac => Environment.OSVersion.Platform == PlatformID.MacOSX;

		public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32S ||
		                                Environment.OSVersion.Platform == PlatformID.Win32Windows ||
		                                Environment.OSVersion.Platform == PlatformID.Win32NT ||
		                                Environment.OSVersion.Platform == PlatformID.WinCE;

		public static string TimeStampMessage(string message)
		{
			return string.IsNullOrEmpty(message) ? message : $"{TimeStamp} {message}";
		}

		public static ColoredMessage TimeStampMessage(ColoredMessage message)
		{
			if (message?.text == null) return null;

			ColoredMessage clone = message.Clone();
			clone.text = TimeStampMessage(clone.text);
			return clone;
		}

		public static ColoredMessage[] TimeStampMessage(ColoredMessage[] message, ConsoleColor color = ConsoleColor.Black)
		{
			if (message == null) return null;

			ColoredMessage[] newMessage = new ColoredMessage[message.Length + 1];
			newMessage[0] = new ColoredMessage($"{TimeStamp} ", color);

			for (int i = 0; i < message.Length; i++) newMessage[i + 1] = message[i].Clone();

			return newMessage;
		}

		public static string GetFullPathSafe(string path)
		{
			return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(path.Trim()) ? Path.GetFullPath(path) : null;
		}
	}
}
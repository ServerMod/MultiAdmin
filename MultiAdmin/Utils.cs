using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public static ColoredMessage[] TimeStampMessage(ColoredMessage[] message, ConsoleColor color = ConsoleColor.White)
		{
			if (message == null) return null;

			ColoredMessage[] newMessage = new ColoredMessage[message.Length + 1];
			newMessage[0] = new ColoredMessage($"{TimeStamp} ", color);

			for (int i = 0; i < message.Length; i++)
				newMessage[i + 1] = message[i]?.Clone();

			return newMessage;
		}

		public static ColoredMessage[] TimeStampMessage(ColoredMessage message, ConsoleColor color = ConsoleColor.White)
		{
			return TimeStampMessage(new ColoredMessage[] {message}, color);
		}

		public static string GetFullPathSafe(string path)
		{
			return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(path.Trim()) ? Path.GetFullPath(path) : null;
		}

		private const char WildCard = '*';

		private static bool StringMatches(string input, string pattern)
		{
			if (input == null && pattern == null)
				return true;

			if (pattern == null)
				return false;

			if (pattern == new string(WildCard, pattern.Length))
				return true;

			if (input == null)
				return false;

			if (input.Length == 0 && pattern.Length == 0)
				return true;

			if (input.Length == 0 || pattern.Length == 0)
				return false;

			string[] wildCardSections = pattern.Split(WildCard);

			int matchIndex = 0;
			foreach (string wildCardSection in wildCardSections)
			{
				if (wildCardSection.Length <= 0)
					continue;

				if (matchIndex < 0 || matchIndex >= pattern.Length)
					return false;

				try
				{
					// new ColoredMessage($"Debug: Matching \"{wildCardSection}\" with \"{input.Substring(matchIndex)}\"...").WriteLine();

					matchIndex = input.IndexOf(wildCardSection, matchIndex);

					if (matchIndex < 0)
						return false;

					matchIndex += wildCardSection.Length;

					// new ColoredMessage($"Debug: Match found! Match end index at {matchIndex}.").WriteLine();
				}
				catch
				{
					return false;
				}
			}

			// new ColoredMessage($"Debug: Done matching. Matches = {matchIndex == input.Length || wildCardSections[wildCardSections.Length - 1].Length <= 0}.").WriteLine();

			return matchIndex == input.Length || wildCardSections[wildCardSections.Length - 1].Length <= 0;
		}

		private static bool FileNamesContains(IEnumerable<string> namePatterns, string input)
		{
			return namePatterns != null && namePatterns.Any(namePattern => StringMatches(input, namePattern));
		}

		// Copied from https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo?view=netframework-4.7.2 with small modifications
		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, params string[] fileNames)
		{
			if (source.FullName == target.FullName)
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Copy each file into it's new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				if (fileNames == null || fileNames.Length <= 0 || FileNamesContains(fileNames, fi.Name))
				{
					// Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
					fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
				}
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				if (fileNames == null || fileNames.Length <= 0 || FileNamesContains(fileNames, diSourceSubDir.Name))
				{
					DirectoryInfo nextTargetSubDir =
						target.CreateSubdirectory(diSourceSubDir.Name);
					CopyAll(diSourceSubDir, nextTargetSubDir);
				}
			}
		}

		public static void CopyAll(string source, string target, params string[] fileNames)
		{
			CopyAll(new DirectoryInfo(source), new DirectoryInfo(target), fileNames);
		}
	}
}
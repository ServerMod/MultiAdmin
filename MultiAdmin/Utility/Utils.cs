using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.Utility
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

		public static ColoredMessage[] TimeStampMessage(ColoredMessage[] message, ConsoleColor? color = null)
		{
			if (message == null) return null;

			ColoredMessage[] newMessage = new ColoredMessage[message.Length + 1];
			newMessage[0] = new ColoredMessage($"{TimeStamp} ", color);

			for (int i = 0; i < message.Length; i++)
				newMessage[i + 1] = message[i]?.Clone();

			return newMessage;
		}

		public static ColoredMessage[] TimeStampMessage(ColoredMessage message, ConsoleColor? color = null)
		{
			return TimeStampMessage(new ColoredMessage[] {message}, color);
		}

		public static string GetFullPathSafe(string path)
		{
			return string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
		}

		private const char WildCard = '*';

		private static bool StringMatches(string input, string pattern)
		{
			if (input == null && pattern == null)
				return true;

			if (pattern == null)
				return false;

			if (!pattern.IsEmpty() && pattern == new string(WildCard, pattern.Length))
				return true;

			if (input == null)
				return false;

			if (input.IsEmpty() && pattern.IsEmpty())
				return true;

			if (input.IsEmpty() || pattern.IsEmpty())
				return false;

			string[] wildCardSections = pattern.Split(WildCard);

			int matchIndex = 0;
			foreach (string wildCardSection in wildCardSections)
			{
				if (wildCardSection.IsEmpty())
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

			// new ColoredMessage($"Debug: Done matching. Matches = {matchIndex == input.Length || wildCardSections[wildCardSections.Length - 1].IsEmpty()}.").WriteLine();

			return matchIndex == input.Length || wildCardSections[wildCardSections.Length - 1].IsEmpty();
		}

		private static bool FileNamesContains(IEnumerable<string> namePatterns, string input)
		{
			return namePatterns != null && namePatterns.Any(namePattern => StringMatches(input, namePattern));
		}

		private static bool PassesWhitelistAndBlacklist(string toCheck, string[] whitelist = null, string[] blacklist = null)
		{
			return (whitelist.IsNullOrEmpty() || FileNamesContains(whitelist, toCheck)) && (blacklist.IsNullOrEmpty() || !FileNamesContains(blacklist, toCheck));
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, string[] fileWhitelist = null, string[] fileBlacklist = null)
		{
			// If the target directory is the same as the source directory 
			if (source.FullName == target.FullName)
				return;

			Directory.CreateDirectory(target.FullName);

			// Copy each file
			foreach (FileInfo file in source.GetFiles())
			{
				if (PassesWhitelistAndBlacklist(file.Name, fileWhitelist, fileBlacklist))
				{
					file.CopyTo(Path.Combine(target.ToString(), file.Name), true);
				}
			}

			// Copy each sub-directory using recursion
			foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
			{
				if (PassesWhitelistAndBlacklist(sourceSubDir.Name, fileWhitelist, fileBlacklist))
				{
					// Begin copying sub-directory
					CopyAll(sourceSubDir, target.CreateSubdirectory(sourceSubDir.Name));
				}
			}
		}

		public static void CopyAll(string source, string target, string[] fileWhitelist = null, string[] fileBlacklist = null)
		{
			CopyAll(new DirectoryInfo(source), new DirectoryInfo(target), fileWhitelist, fileBlacklist);
		}

		public static int CompareVersionStrings(string firstVersion, string secondVersion)
		{
			if (firstVersion == null || secondVersion == null)
				return -1;

			string[] firstVersionNums = firstVersion.Split('.');
			string[] secondVersionNums = secondVersion.Split('.');
			int minVersionLength = Math.Min(firstVersionNums.Length, secondVersionNums.Length);

			for (int i = 0; i < minVersionLength; i++)
			{
				if (!int.TryParse(firstVersionNums[i], out int first) || !int.TryParse(secondVersionNums[i], out int second))
					continue;

				if (first > second)
				{
					return 1;
				}

				if (first < second)
				{
					return -1;
				}
			}

			return 0;
		}
	}
}

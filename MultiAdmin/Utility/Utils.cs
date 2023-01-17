using System;
using System.IO;
using System.Linq;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.Utility
{
	public static class Utils
	{
		public static string DateTime => System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss");

		public static string TimeStamp
		{
			get
			{
				DateTime now = System.DateTime.Now;
				return $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}]";
			}
		}

		public static string TimeStampMessage(string message)
		{
			return string.IsNullOrEmpty(message) ? message : $"{TimeStamp} {message}";
		}

		public static ColoredMessage?[] TimeStampMessage(ColoredMessage?[] message, ConsoleColor? color = null,
			bool cloneMessages = false)
		{
			ColoredMessage?[] newMessage = new ColoredMessage?[message.Length + 1];
			newMessage[0] = new ColoredMessage($"{TimeStamp} ", color);

			if (cloneMessages)
			{
				for (int i = 0; i < message.Length; i++)
					newMessage[i + 1] = message[i]?.Clone();
			}
			else
			{
				for (int i = 0; i < message.Length; i++)
					newMessage[i + 1] = message[i];
			}

			return newMessage;
		}

		public static ColoredMessage?[] TimeStampMessage(ColoredMessage? message, ConsoleColor? color = null,
			bool cloneMessages = false)
		{
			return TimeStampMessage(new ColoredMessage?[] { message }, color, cloneMessages);
		}

		public static string? GetFullPathSafe(string? path)
		{
			return string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
		}

		private const char WildCard = '*';

		public static bool StringMatches(string input, string pattern, char wildCard = WildCard)
		{
			if (input == null && pattern == null)
				return true;

			if (pattern == null)
				return false;

			if (!pattern.IsEmpty() && pattern == new string(wildCard, pattern.Length))
				return true;

			if (input == null)
				return false;

			if (input.IsEmpty() && pattern.IsEmpty())
				return true;

			if (input.IsEmpty() || pattern.IsEmpty())
				return false;

			string[] wildCardSections = pattern.Split(wildCard);

			int matchIndex = 0;
			foreach (string wildCardSection in wildCardSections)
			{
				// If there's a wildcard with nothing on the other side
				if (wildCardSection.IsEmpty())
				{
					continue;
				}

				if (matchIndex < 0 || matchIndex >= input.Length)
					return false;

				Program.LogDebug(nameof(StringMatches),
					$"Matching \"{wildCardSection}\" with \"{input[matchIndex..]}\"...");

				if (matchIndex <= 0 && pattern[0] != wildCard)
				{
					// If the rest of the input string isn't at least as long as the section to match
					if (input.Length - matchIndex < wildCardSection.Length)
						return false;

					// If the input doesn't match this section of the pattern
					if (!input.Equals(wildCardSection, matchIndex, wildCardSection.Length))
						return false;

					matchIndex += wildCardSection.Length;

					Program.LogDebug(nameof(StringMatches), $"Exact match found! Match end index at {matchIndex}.");
				}
				else
				{
					try
					{
						matchIndex = input.IndexOf(wildCardSection, matchIndex);

						if (matchIndex < 0)
							return false;

						matchIndex += wildCardSection.Length;

						Program.LogDebug(nameof(StringMatches), $"Match found! Match end index at {matchIndex}.");
					}
					catch
					{
						return false;
					}
				}
			}

			Program.LogDebug(nameof(StringMatches),
				$"Done matching. Matches = {matchIndex == input.Length || wildCardSections[^1].IsEmpty()}.");

			return matchIndex == input.Length || wildCardSections[^1].IsEmpty();
		}

		public static bool InputMatchesAnyPattern(string input, params string[]? namePatterns)
		{
			return namePatterns != null && namePatterns.Length > 0 && namePatterns.Any(namePattern => StringMatches(input, namePattern));
		}

		private static bool PassesWhitelistAndBlacklist(string toCheck, string[]? whitelist = null,
			string[]? blacklist = null)
		{
			return (whitelist.IsNullOrEmpty() || InputMatchesAnyPattern(toCheck, whitelist)) &&
				   (blacklist.IsNullOrEmpty() || !InputMatchesAnyPattern(toCheck, blacklist));
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, string[]? fileWhitelist = null,
			string[]? fileBlacklist = null)
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

		public static void CopyAll(string source, string target, string[]? fileWhitelist = null,
			string[]? fileBlacklist = null)
		{
			CopyAll(new DirectoryInfo(source), new DirectoryInfo(target), fileWhitelist, fileBlacklist);
		}

		public static int[] StringArrayToIntArray(string[] stringArray)
		{
			lock (stringArray)
			{
				int[] intArray = new int[stringArray.Length];

				for (int i = 0; i < stringArray.Length; i++)
				{
					if (!int.TryParse(stringArray[i], out int intValue))
						continue;

					intArray[i] = intValue;
				}

				return intArray;
			}
		}

		/// <summary>
		/// Compares <paramref name="firstVersion"/> to <paramref name="secondVersion"/>
		/// </summary>
		/// <param name="firstVersion">The version string to compare</param>
		/// <param name="secondVersion">The version string to compare to</param>
		/// <param name="separator">The separator character between version numbers</param>
		/// <returns>
		/// Returns 1 if <paramref name="firstVersion"/> is greater than (or longer if equal) <paramref name="secondVersion"/>,
		/// 0 if <paramref name="firstVersion"/> is exactly equal to <paramref name="secondVersion"/>,
		/// and -1 if <paramref name="firstVersion"/> is less than (or shorter if equal) <paramref name="secondVersion"/>
		/// </returns>
		public static int CompareVersionStrings(string firstVersion, string secondVersion, char separator = '.')
		{
			if (firstVersion == null || secondVersion == null)
				return -1;

			int[] firstVersionNums = StringArrayToIntArray(firstVersion.Split(separator));
			int[] secondVersionNums = StringArrayToIntArray(secondVersion.Split(separator));
			int minVersionLength = Math.Min(firstVersionNums.Length, secondVersionNums.Length);

			// Compare version numbers
			for (int i = 0; i < minVersionLength; i++)
			{
				if (firstVersionNums[i] > secondVersionNums[i])
				{
					return 1;
				}

				if (firstVersionNums[i] < secondVersionNums[i])
				{
					return -1;
				}
			}

			// If all the numbers are the same

			// Compare version lengths
			if (firstVersionNums.Length > secondVersionNums.Length)
				return 1;
			if (firstVersionNums.Length < secondVersionNums.Length)
				return -1;

			// If the versions are perfectly identical, return 0
			return 0;
		}
	}
}

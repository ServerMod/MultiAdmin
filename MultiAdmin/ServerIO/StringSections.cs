using System;
using System.Collections.Generic;
using System.Text;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.ServerIO
{
	public class StringSections
	{
		public StringSection[] Sections { get; }

		public StringSections(StringSection[] sections)
		{
			Sections = sections;
		}

		public StringSection? GetSection(int index, out int sectionIndex)
		{
			sectionIndex = -1;

			for (int i = 0; i < Sections.Length; i++)
			{
				StringSection stringSection = Sections[i];

				if (stringSection.IsWithinSection(index))
				{
					sectionIndex = i;
					return stringSection;
				}
			}

			return null;
		}

		public StringSection? GetSection(int index)
		{
			foreach (StringSection stringSection in Sections)
			{
				if (stringSection.IsWithinSection(index))
					return stringSection;
			}

			return null;
		}

		public static StringSections FromString(string fullString, int sectionLength,
			ColoredMessage? leftIndicator = null, ColoredMessage? rightIndicator = null,
			ColoredMessage? sectionBase = null)
		{
			int rightIndicatorLength = rightIndicator?.Length ?? 0;
			int totalIndicatorLength = (leftIndicator?.Length ?? 0) + rightIndicatorLength;

			if (fullString.Length > sectionLength && sectionLength <= totalIndicatorLength)
				throw new ArgumentException(
					$"{nameof(sectionLength)} must be greater than the total length of {nameof(leftIndicator)} and {nameof(rightIndicator)}",
					nameof(sectionLength));

			List<StringSection> sections = new();

			if (string.IsNullOrEmpty(fullString))
				return new StringSections(sections.ToArray());

			// If the section base message is null, create a default one
			if (sectionBase == null)
				sectionBase = new ColoredMessage(null);

			// The starting index of the current section being created
			int sectionStartIndex = 0;

			// The text of the current section being created
			StringBuilder curSecBuilder = new();

			for (int i = 0; i < fullString.Length; i++)
			{
				curSecBuilder.Append(fullString[i]);

				// If the section is less than the smallest possible section size, skip processing
				if (curSecBuilder.Length < sectionLength - totalIndicatorLength) continue;

				// Decide what the left indicator text should be accounting for the leftmost section
				ColoredMessage? leftIndicatorSection = sections.Count > 0 ? leftIndicator : null;
				// Decide what the right indicator text should be accounting for the rightmost section
				ColoredMessage? rightIndicatorSection =
					i < fullString.Length - (1 + rightIndicatorLength) ? rightIndicator : null;

				// Check the section length against the final section length
				if (curSecBuilder.Length >= sectionLength -
					((leftIndicatorSection?.Length ?? 0) + (rightIndicatorSection?.Length ?? 0)))
				{
					// Copy the section base message and replace the text
					ColoredMessage section = sectionBase.Clone();
					section.text = curSecBuilder.ToString();

					// Instantiate the section with the final parameters
					sections.Add(new StringSection(section, leftIndicatorSection, rightIndicatorSection,
						sectionStartIndex, i));

					// Reset the current section being worked on
					curSecBuilder.Clear();
					sectionStartIndex = i + 1;
				}
			}

			// If there's still text remaining in a section that hasn't been processed, add it as a section
			if (!curSecBuilder.IsEmpty())
			{
				// Only decide for the left indicator, as this last section will always be the rightmost section
				ColoredMessage? leftIndicatorSection = sections.Count > 0 ? leftIndicator : null;

				// Copy the section base message and replace the text
				ColoredMessage section = sectionBase.Clone();
				section.text = curSecBuilder.ToString();

				// Instantiate the section with the final parameters
				sections.Add(new StringSection(section, leftIndicatorSection, null, sectionStartIndex,
					fullString.Length));
			}

			return new StringSections(sections.ToArray());
		}
	}

	public readonly struct StringSection
	{
		public ColoredMessage Text { get; }

		public ColoredMessage? LeftIndicator { get; }
		public ColoredMessage? RightIndicator { get; }

		public ColoredMessage?[] Section => new ColoredMessage?[] { LeftIndicator, Text, RightIndicator };

		public int MinIndex { get; }
		public int MaxIndex { get; }

		public StringSection(ColoredMessage text, ColoredMessage? leftIndicator, ColoredMessage? rightIndicator,
			int minIndex, int maxIndex)
		{
			Text = text;

			LeftIndicator = leftIndicator;
			RightIndicator = rightIndicator;

			MinIndex = minIndex;
			MaxIndex = maxIndex;
		}

		public bool IsWithinSection(int index)
		{
			return index >= MinIndex && index <= MaxIndex;
		}

		public int GetRelativeIndex(int index)
		{
			return index - MinIndex + (LeftIndicator?.Length ?? 0);
		}
	}
}

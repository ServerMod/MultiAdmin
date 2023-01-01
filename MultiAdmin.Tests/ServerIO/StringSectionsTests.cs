using System;
using MultiAdmin.ConsoleTools;
using MultiAdmin.ServerIO;
using Xunit;
using Xunit.Abstractions;

namespace MultiAdmin.Tests.ServerIO
{
	public class StringSectionsTests
	{
		private readonly ITestOutputHelper output;

		public StringSectionsTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[InlineData("test string", new[] { "te", "st", " s", "tr", "in", "g" }, 2)]
		[InlineData("test string", new[] { "tes..", ".t ..", ".st..", ".ring" }, 5, ".", "..")]
		public void FromStringTest(string testString, string[] expectedSections, int sectionLength,
			string? leftIndictator = null, string? rightIndictator = null)
		{
			StringSections sections = StringSections.FromString(testString, sectionLength,
				leftIndictator != null ? new ColoredMessage(leftIndictator) : null,
				rightIndictator != null ? new ColoredMessage(rightIndictator) : null);

			Assert.NotNull(sections);
			Assert.NotNull(sections.Sections);

			Assert.Equal(expectedSections.Length, sections.Sections.Length);

			for (int i = 0; i < expectedSections.Length; i++)
			{
				string expected = expectedSections[i];
				string result = sections.Sections[i].Section.GetText();

				output.WriteLine($"Index {i} - Comparing \"{expected}\" to \"{result}\"...");
				Assert.Equal(expected, result);
			}
		}

		[Theory]
		// No further characters can be output because of the prefix and suffix
		[InlineData("test string", 2, ".", ".")]
		public void FromStringThrowsTest(string testString, int sectionLength, string? leftIndictator = null,
			string? rightIndictator = null)
		{
			Assert.Throws<ArgumentException>(() =>
			{
				StringSections.FromString(testString, sectionLength,
					leftIndictator != null ? new ColoredMessage(leftIndictator) : null,
					rightIndictator != null ? new ColoredMessage(rightIndictator) : null);
			});
		}
	}
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiAdmin.ConsoleTools;
using MultiAdmin.ServerIO;

namespace MultiAdminTests.ServerIO
{
	[TestClass]
	public class StringSectionsTests
	{
		private struct FromStringTemplate
		{
			public readonly string testString;
			public readonly string[] expectedSections;

			public readonly int sectionLength;
			public readonly ColoredMessage leftIndictator;
			public readonly ColoredMessage rightIndictator;

			public FromStringTemplate(string testString, string[] expectedSections, int sectionLength, ColoredMessage leftIndictator = null, ColoredMessage rightIndictator = null)
			{
				this.testString = testString;
				this.expectedSections = expectedSections;

				this.sectionLength = sectionLength;
				this.leftIndictator = leftIndictator;
				this.rightIndictator = rightIndictator;
			}
		}

		[TestMethod]
		public void FromStringTest()
		{
			try
			{
				StringSections.FromString("test string", 2, new ColoredMessage("."), new ColoredMessage("."));
				Assert.Fail("This case should not be allowed, no further characters can be output because of the prefix and suffix");
			}
			catch (ArgumentException)
			{
				// Expected behaviour
			}

			FromStringTemplate[] sectionTests =
			{
				new FromStringTemplate("test string", new string[] {"te", "st", " s", "tr", "in", "g"}, 2),
				new FromStringTemplate("test string", new string[] {"tes..", ".t ..", ".st..", ".ring"}, 5, new ColoredMessage("."), new ColoredMessage(".."))
			};

			for (int i = 0; i < sectionTests.Length; i++)
			{
				FromStringTemplate sectionTest = sectionTests[i];

				StringSections sections = StringSections.FromString(sectionTest.testString, sectionTest.sectionLength, sectionTest.leftIndictator, sectionTest.rightIndictator);

				Assert.IsNotNull(sections);
				Assert.IsNotNull(sections.Sections);

				Assert.IsTrue(sections.Sections.Length == sectionTest.expectedSections.Length, $"Failed at index {i}: Expected sections length \"{sectionTest.expectedSections.Length}\", got \"{sections.Sections.Length}\"");

				for (int j = 0; j < sectionTest.expectedSections.Length; j++)
				{
					string expected = sectionTest.expectedSections[j];
					string result = sections.Sections[j].Section.GetText();

					Assert.AreEqual(expected, result, $"Failed at index {i}: Failed at section index {j}: Expected section text to be \"{expected ?? "null"}\", got \"{result ?? "null"}\"");
				}
			}
		}
	}
}

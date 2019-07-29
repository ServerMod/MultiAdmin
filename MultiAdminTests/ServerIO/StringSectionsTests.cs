using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiAdmin.ServerIO;

namespace MultiAdminTests.ServerIO
{
	[TestClass]
	public class StringSectionsTests
	{
		[TestMethod]
		public void FromStringTest()
		{
			string[] expectedSections =
			{
				"te",
				"st",
				" s",
				"tr",
				"in",
				"g"
			};

			StringSections sections = StringSections.FromString("test string", 2);

			Assert.IsNotNull(sections);
			Assert.IsNotNull(sections.Sections);

			Assert.IsTrue(sections.Sections.Length == expectedSections.Length, $"Expected sections length \"{expectedSections.Length}\", got \"{sections.Sections.Length}\"");

			for (int i = 0; i < expectedSections.Length; i++)
			{
				string expected = expectedSections[i];
				string result = sections.Sections[i].Text?.text;

				Assert.AreEqual(expected, result, $"Failed at section index {i}: Expected section text to be \"{expected ?? "null"}\", got \"{result ?? "null"}\"");
			}
		}
	}
}

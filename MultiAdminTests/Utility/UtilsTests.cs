using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiAdmin.Utility;

namespace MultiAdminTests.Utility
{
	[TestClass]
	public class UtilsTests
	{
		private struct CompareVersionTemplate
		{
			public readonly string firstVersion;
			public readonly string secondVersion;

			public readonly int expectedResult;

			public CompareVersionTemplate(string firstVersion, string secondVersion, int expectedResult)
			{
				this.firstVersion = firstVersion;
				this.secondVersion = secondVersion;
				this.expectedResult = expectedResult;
			}

			public bool CheckResult(int result)
			{
				if (expectedResult == result)
					return true;

				if (expectedResult < 0 && result < 0)
					return true;

				if (expectedResult > 0 && result > 0)
					return true;

				return false;
			}
		}

		[TestMethod]
		public void CompareVersionStringsTest()
		{
			CompareVersionTemplate[] versionTests = {new CompareVersionTemplate("1.0.0.0", "2.0.0.0", -1), new CompareVersionTemplate("1.0.0.0", "1.0.0.0", 0), new CompareVersionTemplate("2.0.0.0", "1.0.0.0", 1), new CompareVersionTemplate("1.0", "2.0.0.0", -1), new CompareVersionTemplate("1.0", "1.0.0.0", 0), new CompareVersionTemplate("2.0", "1.0.0.0", 1), new CompareVersionTemplate("1.0.0.0", "2.0", -1), new CompareVersionTemplate("1.0.0.0", "1.0", 0), new CompareVersionTemplate("2.0.0.0", "1.0", 1), new CompareVersionTemplate("6.0.0.313", "5.18.0", 1), new CompareVersionTemplate("5.18.0", "6.0.0.313", -1), new CompareVersionTemplate("5.18.0", "5.18.0", 0), new CompareVersionTemplate("5.18", "5.18.0", 0)};

			for (int i = 0; i < versionTests.Length; i++)
			{
				CompareVersionTemplate test = versionTests[i];

				int result = Utils.CompareVersionStrings(test.firstVersion, test.secondVersion);

				Assert.IsTrue(test.CheckResult(result), $"{nameof(Utils.CompareVersionStrings)} failed on test index {i}: Expected result \"{test.expectedResult}\", got \"{result}\"");
			}
		}
	}
}

using MultiAdmin.Utility;
using Xunit;

namespace MultiAdmin.Tests.Utility
{
	public class UtilsTests
	{
		[Fact]
		public void GetFullPathSafeTest()
		{
			Assert.Null(Utils.GetFullPathSafe(" "));
		}

		[Theory]
		[InlineData("test", "*", true)]
		[InlineData("test", "te*", true)]
		[InlineData("test", "*st", true)]
		[InlineData("test", "******", true)]
		[InlineData("test", "te*t", true)]
		[InlineData("test", "t**st", true)]
		[InlineData("test", "s*", false)]
		[InlineData("longstringtestmessage", "l*s*t*e*g*", true)]
		[InlineData("AdminToolbox", "config_remoteadmin.txt", false)]
		[InlineData("config_remoteadmin.txt", "config_remoteadmin.txt", true)]
		[InlineData("sizetest", "sizetest1", false)]
		public void StringMatchesTest(string input, string pattern, bool expected)
		{
			bool result = Utils.StringMatches(input, pattern);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("1.0.0.0", "2.0.0.0", -1)]
		[InlineData("1.0.0.0", "1.0.0.0", 0)]
		[InlineData("2.0.0.0", "1.0.0.0", 1)]

		[InlineData("1.0", "2.0.0.0", -1)]
		[InlineData("1.0", "1.0.0.0", -1)] // The first version is shorter, so it's lower
		[InlineData("2.0", "1.0.0.0", 1)]

		[InlineData("1.0.0.0", "2.0", -1)]
		[InlineData("1.0.0.0", "1.0", 1)] // The first version is longer, so it's higher
		[InlineData("2.0.0.0", "1.0", 1)]

		[InlineData("6.0.0.313", "5.18.0", 1)]
		[InlineData("5.18.0", "6.0.0.313", -1)]

		[InlineData("5.18.0", "5.18.0", 0)]
		[InlineData("5.18", "5.18.0", -1)] // The first version is shorter, so it's lower
		public void CompareVersionStringsTest(string firstVersion, string secondVersion, int expected)
		{
			int result = Utils.CompareVersionStrings(firstVersion, secondVersion);

			Assert.Equal(expected, result);
		}
	}
}

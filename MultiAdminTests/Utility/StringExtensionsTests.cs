using MultiAdmin.Utility;
using Xunit;

namespace MultiAdminTests.Utility
{
	public class StringExtensionsTests
	{
		[Fact]
		public void EqualsTest()
		{
			Assert.True("test".Equals("test", startIndex: 0));
			Assert.False("test".Equals("other", startIndex: 0));

			Assert.True("test".Equals("st", startIndex: 2));
			Assert.True("test".Equals("te", 0, 2));

			Assert.False("test".Equals("te", startIndex: 2));
			Assert.False("test".Equals("st", 0, 2));

			Assert.True("test".Equals("es", 1, 2));
		}
	}
}

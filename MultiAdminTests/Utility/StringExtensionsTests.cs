using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiAdmin.Utility;

namespace MultiAdminTests.Utility
{
	[TestClass]
	public class StringExtensionsTests
	{
		[TestMethod]
		public void EqualsTest()
		{
			Assert.IsTrue("test".Equals("test", startIndex: 0));
			Assert.IsFalse("test".Equals("other", startIndex: 0));

			Assert.IsTrue("test".Equals("st", startIndex: 2));
			Assert.IsTrue("test".Equals("te", 0, 2));

			Assert.IsFalse("test".Equals("te", startIndex: 2));
			Assert.IsFalse("test".Equals("st", 0, 2));

			Assert.IsTrue("test".Equals("es", 1, 2));
		}
	}
}

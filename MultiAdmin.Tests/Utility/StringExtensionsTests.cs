using System;
using MultiAdmin.Utility;
using Xunit;

namespace MultiAdmin.Tests.Utility
{
	public class StringExtensionsTests
	{
		[Theory]
		[InlineData("test", "test", 0)]
		[InlineData("test", "test", 0, 4)]
		[InlineData("test", "st", 2)]
		[InlineData("test", "te", 0, 2)]
		[InlineData("test", "es", 1, 2)]
		[InlineData(null, null, 0)]
		[InlineData(null, null, 0, 1)]
		public void EqualsTest(string? main, string? section, int startIndex, int count = -1)
		{
			Assert.True(count < 0 ? main.Equals(section, startIndex) : main.Equals(section, startIndex, count));
		}

		[Theory]
		[InlineData("test", "other", 0, 4)]
		[InlineData("test", "te", 2)]
		[InlineData("test", "st", 0, 2)]
		[InlineData("test", null, 0)]
		[InlineData(null, "test", 0)]
		[InlineData("test", null, 0, 1)]
		[InlineData(null, "test", 0, 1)]
		public void NotEqualsTest(string? main, string? section, int startIndex, int count = -1)
		{
			Assert.False(count < 0 ? main.Equals(section, startIndex) : main.Equals(section, startIndex, count));
		}

		[Theory]
		[InlineData(typeof(ArgumentOutOfRangeException), "longtest", "test", 1, 5)]
		[InlineData(typeof(ArgumentOutOfRangeException), "test", "st", 3)]
		[InlineData(typeof(ArgumentOutOfRangeException), "test", "te", -1)]
		[InlineData(typeof(ArgumentOutOfRangeException), "test", "es", 4)]
		public void EqualsThrowsTest(Type expected, string? main, string? section, int startIndex, int count = -1)
		{
			Assert.Throws(expected, () =>
			{
				if (count < 0)
					main.Equals(section, startIndex);
				else
					main.Equals(section, startIndex, count);
			});
		}
	}
}

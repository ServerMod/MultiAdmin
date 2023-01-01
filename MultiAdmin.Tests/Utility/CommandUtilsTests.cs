using MultiAdmin.Utility;
using Xunit;

namespace MultiAdmin.Tests.Utility
{
	public class CommandUtilsTests
	{
		[Theory]
		[InlineData("test", new string[] { "test" })]
		[InlineData("configgen \"", new string[] { "configgen", "\"" })]
		[InlineData("test something test something", new string[] { "test", "something", "test", "something" })]
		[InlineData("test \"something test\" something", new string[] { "test", "something test", "something" })]
		[InlineData("test \\\"something test\\\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \\\"something test\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test\\\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test something\"", new string[] { "test", "something test something" })]
		[InlineData("\"test something test something\"", new string[] { "test something test something" })]
		[InlineData("test \"something test something\\\"", new string[] { "test", "\"something", "test", "something\"" })]
		public void StringToArgsTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test", 2, 2, new string[] { "st" })]
		[InlineData("configgen \"", 0, 11, new string[] { "configgen", "\"" })]
		[InlineData("configgen \"", 0, 10, new string[] { "configgen", "" })]
		[InlineData("test \"something test\" something\"", 10, 22, new string[] { "thing", "test something" })]
		[InlineData("test \"something \"test something\"", 10, 22, new string[] { "thing", "test something" })]
		public void StringToArgsSubstringTest(string input, int start, int count, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, start, count);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test", new string[] { "test" })]
		[InlineData("configgen\t\"", new string[] { "configgen", "\"" })]
		[InlineData("test\tsomething\ttest\tsomething", new string[] { "test", "something", "test", "something" })]
		[InlineData("test\t\"something\ttest\"\tsomething", new string[] { "test", "something\ttest", "something" })]
		[InlineData("test\t\\\"something\ttest\\\"\tsomething", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test\t\\\"something\ttest\"\tsomething", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test\t\"something\ttest\\\"\tsomething", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test\t\"something\ttest\tsomething\"", new string[] { "test", "something\ttest\tsomething" })]
		[InlineData("\"test\tsomething\ttest\tsomething\"", new string[] { "test\tsomething\ttest\tsomething" })]
		[InlineData("test\t\"something\ttest\tsomething\\\"", new string[] { "test", "\"something", "test", "something\"" })]
		[InlineData("test\t\\\"something  \ttest\"\tsomething", new string[] { "test", "\"something  ", "test\"", "something" })]
		[InlineData("test\t\"something\ttest\\\"\t something", new string[] { "test", "\"something", "test\"", " something" })]
		[InlineData("test \t\"something\ttest\tsomething\"", new string[] { "test ", "something\ttest\tsomething" })]
		[InlineData("\"test something\ttest\tsomething\"", new string[] { "test something\ttest\tsomething" })]
		[InlineData("test\t\"something test\tsomething\\\"", new string[] { "test", "\"something test", "something\"" })]
		public void StringToArgsSeparatorTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, separator: '\t');
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test \\\"something test\\\" something", new string[] { "test", "\\something test\\", "something" })]
		[InlineData("test \\\"something test\" something", new string[] { "test", "\\something test", "something" })]
		[InlineData("test \"something test\\\" something", new string[] { "test", "something test\\", "something" })]
		[InlineData("test \"something test something\"", new string[] { "test", "something test something" })]
		[InlineData("\"test something test something\"", new string[] { "test something test something" })]
		[InlineData("test \"something test something\\\"", new string[] { "test", "something test something\\" })]
		[InlineData("test $\"something test$\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test $\"something test\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test$\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test something$\"", new string[] { "test", "\"something", "test", "something\"" })]
		public void StringToArgsEscapeTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, escapeChar: '$');
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test \\\'something test\\\' something", new string[] { "test", "\'something", "test\'", "something" })]
		[InlineData("test \\\'something test\' something", new string[] { "test", "\'something", "test\'", "something" })]
		[InlineData("test \'something test\\\' something", new string[] { "test", "\'something", "test\'", "something" })]
		[InlineData("test \'something test something\'", new string[] { "test", "something test something" })]
		[InlineData("\'test something test something\'", new string[] { "test something test something" })]
		[InlineData("test \'something test something\\\'", new string[] { "test", "\'something", "test", "something\'" })]
		public void StringToArgsQuotesTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, quoteChar: '\'');
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test \\\"something test\\\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \\\"something test\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test\\\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test something\"", new string[] { "test", "\"something test something\"" })]
		[InlineData("\"test something test something\"", new string[] { "\"test something test something\"" })]
		[InlineData("test \"something test something\\\"", new string[] { "test", "\"something", "test", "something\"" })]
		public void StringToArgsKeepQuotesTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, keepQuotes: true);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("test \"something test something\"", new string[] { "test", "something test something" })]
		[InlineData("test \"\"something test something\"\"", new string[] { "test", "\"something", "test", "something\"" })]
		[InlineData("test \"\"something test\"\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"\"something test\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("test \"something test\"\" something", new string[] { "test", "\"something", "test\"", "something" })]
		[InlineData("\"test something test something\"", new string[] { "test something test something" })]
		[InlineData("test \"something test\"\" something\" test test \"test test\" something \"something\" test",
			new string[] { "test", "something test\" something", "test", "test", "test test", "something", "something", "test" })]
		public void StringToArgsDoubleEscapeTest(string input, string[] expected)
		{
			string[] result = CommandUtils.StringToArgs(input, escapeChar: '\"', quoteChar: '\"');
			Assert.Equal(expected, result);
		}
	}
}

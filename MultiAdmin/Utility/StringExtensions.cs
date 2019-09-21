using System;

namespace MultiAdmin.Utility
{
	public static class StringExtensions
	{
		public static bool Equals(this string input, string value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count < 0 || startIndex > input.Length - count)
				throw new ArgumentOutOfRangeException(nameof(count));

			for (int i = 0; i < count; i++)
			{
				int curIndex = startIndex + i;

				if (input[curIndex] != value[i])
					return false;
			}

			return true;
		}

		public static bool Equals(this string input, string value, int startIndex)
		{
			return Equals(input, value, startIndex, input.Length - startIndex);
		}
	}
}

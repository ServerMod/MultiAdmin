using System;

namespace MultiAdmin.Utility
{
	public static class StringExtensions
	{
		public static bool Equals(this string input, string value, int startIndex, int count)
		{
			if (input == null && value == null)
				return true;
			if (input == null || value == null)
				return false;

			if (startIndex < 0 || startIndex >= input.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count < 0 || count > value.Length || startIndex > input.Length - count)
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
			if (input == null && value == null)
				return true;
			if (input == null || value == null)
				return false;

			int length = input.Length - startIndex;

			if (length < value.Length)
				throw new ArgumentOutOfRangeException(nameof(value));

			return Equals(input, value, startIndex, length);
		}
	}
}

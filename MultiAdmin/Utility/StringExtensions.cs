using System;

namespace MultiAdmin.Utility
{
	public static class StringExtensions
	{
		public static bool Equals(this string? input, string? value, int startIndex, int count)
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
				if (input[startIndex + i] != value[i])
					return false;
			}

			return true;
		}

		public static bool Equals(this string? input, string? value, int startIndex)
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

		/// <summary>
		/// Escapes this <see cref="string"/> for use with <see cref="string.Format"/>
		/// </summary>
		/// <param name="input">The <see cref="string"/> to escape</param>
		/// <returns>A <see cref="string"/> escaped for use with <see cref="string.Format"/></returns>
		public static string EscapeFormat(this string input)
		{
			return input.Replace("{", "{{").Replace("}", "}}");
		}
	}
}

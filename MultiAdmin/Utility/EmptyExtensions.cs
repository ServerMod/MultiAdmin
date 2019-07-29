using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiAdmin.Utility
{
	public static class EmptyExtensions
	{
		public static bool IsEmpty<T>(this IEnumerable<T> iEnumerable)
		{
			return !iEnumerable.Any();
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> iEnumerable)
		{
			return iEnumerable?.IsEmpty() ?? true;
		}

		public static bool IsEmpty(this Array array)
		{
			return array.Length <= 0;
		}

		public static bool IsNullOrEmpty(this Array array)
		{
			return array?.IsEmpty() ?? true;
		}

		public static bool IsEmpty<T>(this T[] array)
		{
			return array.Length <= 0;
		}

		public static bool IsNullOrEmpty<T>(this T[] array)
		{
			return array?.IsEmpty() ?? true;
		}

		public static bool IsEmpty<T>(this ICollection<T> collection)
		{
			return collection.Count <= 0;
		}

		public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
		{
			return collection?.IsEmpty() ?? true;
		}

		public static bool IsEmpty<T>(this List<T> list)
		{
			return list.Count <= 0;
		}

		public static bool IsNullOrEmpty<T>(this List<T> list)
		{
			return list?.IsEmpty() ?? true;
		}

		public static bool IsEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			return dictionary.Count <= 0;
		}

		public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			return dictionary?.IsEmpty() ?? true;
		}
	}
}

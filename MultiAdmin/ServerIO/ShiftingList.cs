using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MultiAdmin.ServerIO
{
	public class ShiftingList : ReadOnlyCollection<string>
	{
		public int MaxCount { get; }

		public ShiftingList(int maxCount) : base(new List<string>(maxCount))
		{
			if (maxCount <= 0)
				throw new ArgumentException("The maximum index count can not be less than or equal to zero.");

			MaxCount = maxCount;
		}

		private void LimitLength()
		{
			while (Items.Count > MaxCount)
			{
				RemoveFromEnd();
			}
		}

		public void Add(string item, int index = 0)
		{
			lock (Items)
			{
				Items.Insert(index, item);

				LimitLength();
			}
		}

		public void Remove(string item)
		{
			lock (Items)
			{
				Items.Remove(item);
			}
		}

		public void RemoveFromEnd()
		{
			lock (Items)
			{
				Items.RemoveAt(Items.Count - 1);
			}
		}

		public void RemoveAt(int index)
		{
			lock (Items)
			{
				Items.RemoveAt(index);
			}
		}

		public void Replace(string item, int index = 0)
		{
			lock (Items)
			{
				RemoveAt(index);
				Add(item, index);
			}
		}
	}
}

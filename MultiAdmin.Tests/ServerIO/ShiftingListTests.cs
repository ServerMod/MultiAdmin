using System;
using MultiAdmin.ServerIO;
using Xunit;

namespace MultiAdmin.Tests.ServerIO
{
	public class ShiftingListTests
	{
		[Fact]
		public void ShiftingListTest()
		{
			const int maxCount = 2;
			ShiftingList shiftingList = new(maxCount);

			Assert.Equal(maxCount, shiftingList.MaxCount);
		}

		[Fact]
		public void AddTest()
		{
			const int maxCount = 2;
			const int entriesToAdd = 6;
			ShiftingList shiftingList = new(maxCount);

			for (int i = 0; i < entriesToAdd; i++)
			{
				shiftingList.Add($"Test{i}");
			}

			Assert.Equal(maxCount, shiftingList.Count);

			for (int i = 0; i < shiftingList.Count; i++)
			{
				Assert.Equal($"Test{entriesToAdd - i - 1}", shiftingList[i]);
			}
		}

		[Fact]
		public void RemoveFromEndTest()
		{
			const int maxCount = 6;
			const int entriesToRemove = 2;
			ShiftingList shiftingList = new(maxCount);

			for (int i = 0; i < maxCount; i++)
			{
				shiftingList.Add($"Test{i}");
			}

			for (int i = 0; i < entriesToRemove; i++)
			{
				shiftingList.RemoveFromEnd();
			}

			Assert.Equal(Math.Max(maxCount - entriesToRemove, 0), shiftingList.Count);

			for (int i = 0; i < shiftingList.Count; i++)
			{
				Assert.Equal($"Test{maxCount - i - 1}", shiftingList[i]);
			}
		}

		[Fact]
		public void ReplaceTest()
		{
			const int maxCount = 6;
			const int indexToReplace = 2;
			ShiftingList shiftingList = new(maxCount);

			for (int i = 0; i < maxCount; i++)
			{
				shiftingList.Add($"Test{i}");
			}

			for (int i = 0; i < maxCount; i++)
			{
				if (i == indexToReplace)
				{
					shiftingList.Replace("Replaced", indexToReplace);
				}
			}

			Assert.Equal(maxCount, shiftingList.Count);

			Assert.Equal("Replaced", shiftingList[indexToReplace]);
		}
	}
}

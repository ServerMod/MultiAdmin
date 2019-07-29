using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiAdmin.ServerIO;

namespace MultiAdminTests.ServerIO
{
	[TestClass]
	public class ShiftingListTests
	{
		[TestMethod]
		public void ShiftingListTest()
		{
			const int maxCount = 2;
			ShiftingList shiftingList = new ShiftingList(maxCount);

			Assert.AreEqual(shiftingList.MaxCount, maxCount);
		}

		[TestMethod]
		public void AddTest()
		{
			const int maxCount = 2;
			const int entriesToAdd = 6;
			ShiftingList shiftingList = new ShiftingList(maxCount);

			for (int i = 0; i < entriesToAdd; i++)
			{
				shiftingList.Add($"Test{i}");
			}

			Assert.AreEqual(shiftingList.Count, maxCount);

			for (int i = 0; i < shiftingList.Count; i++)
			{
				Assert.AreEqual(shiftingList[i], $"Test{entriesToAdd - i - 1}");
			}
		}

		[TestMethod]
		public void RemoveFromEndTest()
		{
			const int maxCount = 6;
			const int entriesToRemove = 2;
			ShiftingList shiftingList = new ShiftingList(maxCount);

			for (int i = 0; i < maxCount; i++)
			{
				shiftingList.Add($"Test{i}");
			}

			for (int i = 0; i < entriesToRemove; i++)
			{
				shiftingList.RemoveFromEnd();
			}

			Assert.AreEqual(shiftingList.Count, Math.Max(maxCount - entriesToRemove, 0));

			for (int i = 0; i < shiftingList.Count; i++)
			{
				Assert.AreEqual(shiftingList[i], $"Test{maxCount - i - 1}");
			}
		}

		[TestMethod]
		public void ReplaceTest()
		{
			const int maxCount = 6;
			const int indexToReplace = 2;
			ShiftingList shiftingList = new ShiftingList(maxCount);

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

			Assert.AreEqual(shiftingList.Count, maxCount);

			Assert.AreEqual(shiftingList[indexToReplace], "Replaced");
		}
	}
}

using BGC.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Tests
{
    public class AntiSortTest
    {
        [Test]
        public void TestAntiSort()
        {
            List<int> sorted1 = new() { 1, 2, 3, 4 };
            Assert.IsFalse(IsAntiSorted(sorted1));

            List<int> sorted2 = new() { 1, 1, 2, 2, 3 };
            Assert.IsFalse(IsAntiSorted(sorted1));

            List<int> sorted3 = new() { 5, 4, 3, 2, 1, 2 };
            Assert.IsFalse(IsAntiSorted(sorted1));

            List<int> sorted4 = new() { 4, 5, 4, 3, 2, 1 };
            Assert.IsFalse(IsAntiSorted(sorted4));

            List<int> sorted5 = new() { 1, 1, 1 };
            Assert.IsFalse(IsAntiSorted(sorted5));

            List<float> testList1 = new() { 2f, 4f, 6f, 7f, 8f, 9f, 10f, 12f, 16f, 17f, 80f };
            testList1.AntiSort();
            Assert.IsTrue(IsAntiSorted(testList1));
            Assert.IsTrue(string.Join(", ", testList1) == "2, 6, 4, 7, 9, 8, 10, 16, 12, 80, 17");

            List<char> testList2 = new() { 'a', 'e', 'f', 'c', 'g', 'z', 'w', 'x', };
            testList2.AntiSort();
            Assert.IsTrue(IsAntiSorted(testList2));
            Assert.IsTrue(string.Join(", ", testList2) == "a, e, c, f, z, g, x, w");

            List<int> testList3 = new() { 1, 2, 3, 4, 5 };
            testList3.AntiSort();
            Assert.IsTrue(IsAntiSorted(testList3));
            Assert.IsTrue(string.Join(", ", testList3) == "1, 3, 2, 5, 4");

            // Edge cases
            List<int> emptyList = new();
            emptyList.AntiSort();
            Assert.IsTrue(emptyList.Count == 0);

            List<int> oneItem = new() { 1 };
            oneItem.AntiSort();
            Assert.IsTrue(oneItem.Count == 1);
            Assert.IsTrue(oneItem[0] == 1);

            List<int> twoItemsSorted = new() { 1, 2 };
            twoItemsSorted.AntiSort();
            Assert.IsTrue(twoItemsSorted.Count == 2);
            Assert.IsTrue(twoItemsSorted[0] == 2);
            Assert.IsTrue(twoItemsSorted[1] == 1);

            List<int> twoItemsUnSorted = new() { 2, 1 };
            twoItemsUnSorted.AntiSort();
            Assert.IsTrue(twoItemsSorted.Count == 2);
            Assert.IsTrue(twoItemsSorted[0] == 2);
            Assert.IsTrue(twoItemsSorted[1] == 1);

            List<int> twoIdenticalItems = new() { 1, 1 };
            twoIdenticalItems.AntiSort();
            Assert.IsTrue(twoIdenticalItems.Count == 2);
            Assert.IsTrue(twoIdenticalItems[0] == 1);
            Assert.IsTrue(twoIdenticalItems[1] == 1);

            List<int> threeIdenticalItems = new() { 1, 1, 1 };
            threeIdenticalItems.AntiSort();
            Assert.IsTrue(threeIdenticalItems.Count == 3);
            Assert.IsTrue(threeIdenticalItems[0] == 1);
            Assert.IsTrue(threeIdenticalItems[1] == 1);
            Assert.IsTrue(threeIdenticalItems[2] == 1);

            List<int> someIdenticalItems = new() { 1, 1, 2, 2, 3 };
            someIdenticalItems.AntiSort();
            Assert.IsTrue(someIdenticalItems.Count == 5);
            Assert.IsTrue(string.Join(", ", someIdenticalItems) == "2, 1, 3, 2, 1");

            List<TestAntiSortHiddenDataClass> testCustomCompare1 = new() { new(1), new(2), new(3), new(4), new(5) };
            testCustomCompare1.AntiSort((a, b) => a.Id.CompareTo(b.Id));
            Assert.IsTrue(IsAntiSorted(testCustomCompare1, (a, b) => a.Id.CompareTo(b.Id)));
            Assert.IsTrue(string.Join(", ", testCustomCompare1) == "1, 3, 2, 5, 4");

            List<TestAntiSortHiddenDataClass> testCustomCompare2 = new() { new(1), new(1), new(2), new(2), new(3) };
            testCustomCompare2.AntiSort((a, b) => a.Id.CompareTo(b.Id));
            Assert.IsTrue(IsAntiSorted(testCustomCompare2, (a, b) => a.Id.CompareTo(b.Id)));
            Assert.IsTrue(string.Join(", ", testCustomCompare2) == "2, 1, 3, 2, 1");
        }

        private bool IsAntiSorted<T>(List<T> list)
        {
            return IsAntiSorted(list, Comparer<T>.Default);
        }

        private bool IsAntiSorted<T>(List<T> list, Comparison<T> comparison)
        {
            return IsAntiSorted(list, Comparer<T>.Create(comparison));
        }

        private bool IsAntiSorted<T>(List<T> list, IComparer<T> comparer)
        {
            List<T> sortedList = list.ToList();
            sortedList.Sort(comparer);

            for (int i = 0; i < list.Count - 1; i++)
            {
                T a = list[i];
                T b = list[i + 1];
                if (comparer.Compare(a, b) == 0)
                {
                    return false;
                }

                int aIndex = -1;
                for (int curAIndex = sortedList.Count - 1; curAIndex >= 0; curAIndex--)
                {
                    if (comparer.Compare(sortedList[curAIndex], a) == 0)
                    {
                        aIndex = curAIndex;
                        break;
                    }
                }
                Assert.IsTrue(aIndex != -1);

                if (aIndex != sortedList.Count - 1)
                {
                    T aNext = sortedList[aIndex + 1];
                    if (comparer.Compare(b, aNext) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private class TestAntiSortHiddenDataClass
        {
            public TestAntiSortHiddenDataClass(int id)
            {
                Id = id;
            }

            public int Id { get; private set; }

            public override string ToString()
            {
                return Id.ToString();
            }
        }
    }
}


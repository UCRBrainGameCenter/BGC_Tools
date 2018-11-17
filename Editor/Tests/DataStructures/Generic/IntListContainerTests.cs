using System.Collections.Generic;
using BGC.DataStructures;
using NUnit.Framework;
using LightJson;

namespace BGC.Tests
{
    public class IntListContainerTests
    {
        [Test]
        public void TestBracketAccess()
        {
            IntListContainer lw = new IntListContainer(0, 1, 2, 3, 3, 4, 5, 5, 6, 7);
            Assert.AreEqual(0, lw[0]);
            Assert.AreEqual(1, lw[1]);
        }

        [Test]
        public void TestCount()
        {
            IntListContainer lw = new IntListContainer(0, 1, 2, 3, 3, 4, 5, 5, 6, 7);
            Assert.AreEqual(10, lw.Count);

            lw = new IntListContainer();
            Assert.AreEqual(0, lw.Count);
        }

        [Test]
        public void TestEquals()
        {
            IntListContainer lw1 = new IntListContainer(0, 1, 2, 3, 3, 4);
            IntListContainer lw2 = new IntListContainer(0, 6, 7);
            Assert.AreNotEqual(lw1.list, lw2.list);
            Assert.AreNotEqual(lw1, lw2);
            Assert.IsFalse(lw1.Equals(lw2));

            lw1 = new IntListContainer(0, 1, 2, 3, 3, 4, 5, 5, 6, 7);
            lw2 = new IntListContainer(0, 1, 2, 3, 3, 4, 5, 5, 6, 7);
            Assert.AreEqual(lw1.list, lw2.list);
            Assert.AreEqual(lw1, lw2);
            Assert.IsTrue(lw1.Equals(lw2));
        }

        [Test]
        public void TestListConstructor()
        {
            List<int> testList1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            List<int> testList2 = new List<int>();
            List<int> nullList = null;

            IntListContainer list1 = new IntListContainer(testList1);
            IntListContainer list2 = new IntListContainer(testList2);
            IntListContainer list3 = new IntListContainer(nullList);

            int count = testList1.Count;
            int i;
            Assert.AreEqual(count, list1.Count);
            for (i = 0; i < count; ++i)
            {
                Assert.AreEqual(testList1[i], list1[i]);
            }

            Assert.AreEqual(0, list2.Count);
            Assert.AreEqual(0, list3.Count);
        }

        [Test]
        public void TestArrayConstructor()
        {
            int[] test1 = new int[] { 10, 5, 3 };
            int[] test2 = new int[] { };
            int[] nullTest = null;

            IntListContainer list1 = new IntListContainer(test1);
            IntListContainer list2 = new IntListContainer(test2);
            IntListContainer nullList = new IntListContainer(nullTest);
            IntListContainer paramTest = new IntListContainer(10, 5, 3);

            int count = test1.Length;
            int i;
            Assert.AreEqual(count, list1.Count);

            for (i = 0; i < count; ++i)
            {
                Assert.AreEqual(test1[i], list1[i]);
            }

            Assert.AreEqual(0, list2.Count);
            Assert.AreEqual(0, nullList.Count);

            Assert.AreEqual(count, paramTest.Count);
            for (i = 0; i < count; ++i)
            {
                Assert.AreEqual(list1[i], paramTest[i]);
            }
        }

        [Test]
        public void TestJsonArrayConstructor()
        {
            JsonArray test1 = new JsonArray() { 1123, 5, 33, 5, 10 };
            JsonArray test2 = new JsonArray();
            JsonArray test3 = null;

            IntListContainer list1 = new IntListContainer(test1);
            IntListContainer list2 = new IntListContainer(test2);
            IntListContainer list3 = new IntListContainer(test3);

            int count = test1.Count;
            Assert.AreEqual(count, list1.Count);

            for (int i = 0; i < count; ++i)
            {
                Assert.AreEqual(test1[i].AsInteger, list1[i]);
            }

            Assert.AreEqual(0, list2.Count);
            Assert.AreEqual(0, list3.Count);
        }
    }
}
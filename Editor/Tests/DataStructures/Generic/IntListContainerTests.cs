using System.Collections.Generic;
using UnityEngine.TestTools;
using BGC.DataStructures;
using NUnit.Framework;
using BGC.Extensions;
using UnityEngine;
using LightJson;
using System;

namespace BGC.Tests
{
    public class IntListContainerTests
    {
        [Test]
        public void TestBracketAccess()
        {
            IntListContainer lw = new IntListContainer(0, 1, 2, 3, 4, 5, 6, 7);
            for (int i = 0; i < lw.Count; ++i)
            {
                Assert.AreEqual(i, lw[i]);
            }
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
            JsonArray test4 = new JsonArray() { "hi", "world", new JsonObject() { { "uh", "oh" } } };

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

            try
            {
#pragma warning disable 0219
                IntListContainer list4 = new IntListContainer(test4);
#pragma warning restore 0219
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestEmptyConstructor()
        {
            Assert.AreEqual(0, new IntListContainer().Count);
        }

        [Test]
        public void TestClone()
        {
            IntListContainer list1 = new IntListContainer(1, 2, 3, 4, 5, 6, 7);
            IntListContainer clone = list1.Clone;

            Assert.IsTrue(list1.Equals(clone));
            list1[0] = 100;
            Assert.IsFalse(list1.Equals(clone));
        }

        [Test]
        public void TestRandomValue()
        {
            IntListContainer list = new IntListContainer(0, 1, 2, 3, 4, 5, 6, 76, 8, 98, 10);
            int randomValue = list.RandomValue;
            bool passed = false;

            for (int i = 0; i < 1000; ++i)
            {
                if (randomValue != list.RandomValue)
                {
                    passed = true;
                    break;
                }
            }

            Assert.IsTrue(passed);
        }

        [Test]
        public void TestAdd()
        {
            IntListContainer list = new IntListContainer();
            list.Add(0);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0]);

            list.Add(1);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[1]);
        }

        [Test]
        public void TestRemoveAt()
        {
            IntListContainer list = new IntListContainer(0, 1, 2, 77, 3, 4);
            list.RemoveAt(3);

            Assert.AreEqual(5, list.Count);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(i, list[i]);
            }
        }

        [Test]
        public void TestJsonArray()
        {
            IntListContainer list1 = new IntListContainer(0, 1, 2, 3, 4, 5, 6);
            IntListContainer list2 = new IntListContainer();

            JsonArray array1 = new JsonArray() { 0, 1, 2, 3, 4, 5, 6 };
            JsonArray array2 = new JsonArray();

            Assert.AreEqual(array1.Count, list1.Count);
            for (int i = 0; i < list1.Count; ++i)
            {
                Assert.AreEqual(array1[i].AsInteger, list1[i]);
            }

            Assert.AreEqual(array2.Count, list2.Count);
        }

        [Test]
        public void TestRemoveInt()
        {
            IntListContainer list1 = new IntListContainer(0, 1, 2, 3, 4, 5, 6, 6);
            Assert.IsTrue(list1.Remove(6));
            Assert.IsTrue(list1.Remove(6));
            Assert.IsFalse(list1.Contains(6));

            IntListContainer list2 = new IntListContainer();
            Assert.IsFalse(list2.Remove(0));
        }

        [Test]
        public void TestRemoveList()
        {
            IntListContainer list1 = new IntListContainer(0, 1, 2, 3, 4, 5, 6, 6, 7);
            IntListContainer blackList = new IntListContainer(6, 7, 6);

            Assert.IsTrue(list1.Remove(blackList));
            Assert.IsFalse(list1.Contains(6));
            Assert.IsFalse(list1.Contains(7));
            for (int i = 0; i < 6; ++i)
            {
                Assert.IsTrue(list1.Contains(i));
            }

            list1.Remove(new IntListContainer());
            for (int i = 0; i < 6; ++i)
            {
                Assert.IsTrue(list1.Contains(i));
            }
        }

        [Test]
        public void TestGetHashCode()
        {
            IntListContainer list1 = new IntListContainer(0, 5123, 12, 1234, 1234, 1234, 1234, 12, 2, 5);
            List<int> list = new List<int>() { 0, 5123, 12, 1234, 1234, 1234, 1234, 12, 2, 5 };

            Assert.AreEqual(list.GetSequenceHashCode(), list1.GetHashCode());
            Assert.AreEqual(new List<int>().GetSequenceHashCode(), new IntListContainer().GetHashCode());
        }

        [Test]
        public void TestPrintSelf()
        {
            IntListContainer list1 = new IntListContainer(0, 1, 2, 3, 4, 5);
            IntListContainer list2 = new IntListContainer();

            LogAssert.Expect(LogType.Log, "0) 0");
            LogAssert.Expect(LogType.Log, "1) 1");
            LogAssert.Expect(LogType.Log, "2) 2");
            LogAssert.Expect(LogType.Log, "3) 3");
            LogAssert.Expect(LogType.Log, "4) 4");
            LogAssert.Expect(LogType.Log, "5) 5");
            list1.PrintSelf();

            LogAssert.NoUnexpectedReceived();
            list2.PrintSelf();
        }

        [Test]
        public void TestContains()
        {
            IntListContainer list1 = new IntListContainer(1, 2, 3, 4, 5, 65, 6, 8);
            IntListContainer list2 = new IntListContainer();

            Assert.IsTrue(list1.Contains(1));
            Assert.IsTrue(list1.Contains(65));
            Assert.IsFalse(list1.Contains(-1));
            Assert.IsFalse(list2.Contains(0));
        }
    }
}
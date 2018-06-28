using System.Collections.Generic;
using NUnit.Framework;
using BGC.DataStructures.Generic;

namespace BGC.Tests
{
    public class BagTests
    {
        private readonly List<int> sampleList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        [Test]
        public void PsEmptyTests()
        {
            DepletableBag<int> bag = new DepletableBag<int>(sampleList);
            Assert.IsFalse(bag.Count <= 0);

            for (int i = 0; i < sampleList.Count - 1; ++i)
            {
                bag.PopNext();
                Assert.IsFalse(bag.Count <= 0);
            }

            bag.PopNext();
            Assert.IsTrue(bag.Count <= 0);
        }

        [Test]
        public void PullTests()
        {
            DepletableBag<int> bag = new DepletableBag<int>(sampleList);
            Dictionary<int, bool> dictionary = new Dictionary<int, bool>();

            for (int i = 0; i < sampleList.Count; ++i)
            {
                dictionary.Add(i, false);
            }

            while (bag.Count > 0)
            {
                dictionary[bag.PopNext()] = true;
            }

            for (int i = 0; i < sampleList.Count; ++i)
            {
                Assert.IsTrue(dictionary[i + 1]);
            }

            for (int i = 0; i < 100; ++i)
            {
                if (i >= sampleList.Count)
                {
                    Assert.AreEqual(default(int), bag.PopNext());
                }
            }
        }

        [Test]
        public void PullAutoResetTests()
        {
            DepletableBag<int> bag = new DepletableBag<int>(sampleList);
            bag.AutoRefill = true;

            for (int i = 0; i < 100; ++i)
            {
                Assert.AreNotEqual(default(int), bag.PopNext());
            }
        }
    }
}

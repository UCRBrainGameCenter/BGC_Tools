using System.Collections.Generic;
using BGC.Extensions;
using NUnit.Framework;

namespace BGC.Tests
{
    public class RandomValueTests
    {
        private readonly List<int> sampleList = new List<int>() { 1, 2, 3 };
        private readonly List<int> emptyList = new List<int>() { };

        [Test]
        public void RVTestMaxIndicies()
        {
            int rv = sampleList.RandomValue<int>(0, 1, 2);

            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                "Recieved array of excludedIndicies that does not allow for any values to be returned, " +
                "returning default value");
            Assert.IsTrue(rv == default(int));
        }

        [Test]
        public void RVTestOutOfRangeIndicies()
        {
            int rv = sampleList.RandomValue<int>(-2, -1, 3, 4);

            Assert.IsTrue(rv > 0 && rv < 4);
        }

        [Test]
        public void RVTestAllButOneIndicies()
        {
            int rv = sampleList.RandomValue<int>(0, 2);

            Assert.IsTrue(rv == 2);
        }

        [Test]
        public void RVTestNoIndicies()
        {
            int rv = sampleList.RandomValue<int>();

            Assert.IsTrue(rv > 0 && rv < 4);
        }

        [Test]
        public void RVTestEmptyList()
        {
            int rv = emptyList.RandomValue<int>();

            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                "Received list of length 0 which doesn't allow for random value, " +
                "returning default value");
            Assert.IsTrue(rv == default(int));
        }
    }
}

using System.Collections.Generic;
using BGC.Extensions;
using NUnit.Framework;

public class RandomValueTests
{
    private List<int> sampleList = new List<int>() { 1, 2, 3};

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
        int rv = sampleList.RandomValue<int>(-1, 3);
        Assert.IsTrue(rv > 0 && rv < 4);
    }

    [Test]
    public void RVTestAllButOneIndicies()
    {
        int rv = sampleList.RandomValue<int>(0, 2);
        Assert.IsTrue(rv == 2);
    }
}

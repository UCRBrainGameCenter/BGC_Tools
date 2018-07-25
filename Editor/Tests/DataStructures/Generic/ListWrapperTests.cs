using System.Collections.Generic;
using NUnit.Framework;

public class ListWrapperTests 
{
    [Test]
    public void TestBracketAccess()
    {
        ListWrapper lw = new ListWrapper(new List<int>() { 0,1,2,3,3,4,5,5,6,7 });
        Assert.AreEqual(0, lw[0]);
        Assert.AreEqual(1, lw[1]);
    }

    [Test]
    public void TestCount()
    {
        ListWrapper lw = new ListWrapper(new List<int>() { 0, 1, 2, 3, 3, 4, 5, 5, 6, 7 });
        Assert.AreEqual(10, lw.Count);

        lw = new ListWrapper(new List<int>() { });
        Assert.AreEqual(0, lw.Count);
    }

    [Test]
    public void TestEquals()
    {
        ListWrapper lw1 = new ListWrapper(new List<int>() { 0, 1, 2, 3, 3, 4 });
        ListWrapper lw2 = new ListWrapper(new List<int>() { 0, 6, 7 });
        Assert.AreNotEqual(lw1.list, lw2.list);
        Assert.AreNotEqual(lw1, lw2);

        lw1 = new ListWrapper(new List<int>() { 0, 1, 2, 3, 3, 4, 5, 5, 6, 7 });
        lw2 = new ListWrapper(new List<int>() { 0, 1, 2, 3, 3, 4, 5, 5, 6, 7 });
        Assert.AreEqual(lw1.list, lw2.list);
        Assert.AreEqual(lw1, lw2);
    }
}
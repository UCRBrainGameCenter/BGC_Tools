using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using BGC.Extensions;

namespace BGC.Tests
{
    public class BinarySearchTest
    {
        [Test]
        public void TestBinarySearch()
        {
            float[] testArray = new float[] { 2f, 4f, 6f, 7f, 8f, 9f, 10f, 12f, 16f, 17f, 80f };

            float[] inputValues = new float[]
            { 1f, 2f, 2.1f, 4f, 4.1f, 5.99f, 6f, 8.5f, 16.1f, 17f, 50f, 80f, 81f };
            int[] outputValues = new int[]
            { -1, 0, 0, 1, 1, 1, 2, 4, 8, 9, 9, 10, 10 };

            for (int i = 0; i < inputValues.Length; i++)
            {
                Assert.IsTrue(testArray.BinarySearchLowerBound(inputValues[i]) == outputValues[i],
                    $"Failed Binary Search Test {i}. " +
                    $"Expected {outputValues[i]}, " +
                    $"Found {testArray.BinarySearchLowerBound(inputValues[i])}");
            }
        }
    }
}


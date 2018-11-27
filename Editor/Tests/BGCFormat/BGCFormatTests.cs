using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System;

public class BGCFormatTests
{
    #region LongParse Test

    /// <summary>
    /// A test to demonstrate the outcome of parsing longs.  For peace of mind and demonstration.
    /// </summary>
    [Test]
    public void LongParseTest()
    {
        long[] values = new long[] { int.MaxValue, int.MinValue, 0, 100, long.MaxValue, long.MaxValue, (long.MaxValue / 2) };
        bool[] intParsable = new bool[] { true, true, true, true, false, false, false };
        int[] parsedIntValues = new int[] { int.MaxValue, int.MinValue, 0, 100 };
        bool[] floatParsable = new bool[] { true, true, true, true, true, true, true };

        for (int i = 0; i < values.Length; i++)
        {
            int intValue;
            float floatValue;
            string strValue = values[i].ToString();

            bool intParsed = int.TryParse(strValue, out intValue);
            Assert.IsTrue(intParsed == intParsable[i]);

            if (intParsed)
            {
                Assert.IsTrue(intValue == parsedIntValues[i]);
            }

            bool floatParsed = float.TryParse(strValue, out floatValue);
            Assert.IsTrue(floatParsed == floatParsable[i]);
        }
    }

    #endregion LongParse Test
    #region BoolParse Test

    /// <summary>
    /// A test to demonstrate the outcome of parsing booleans.  For peace of mind and demonstration.
    /// </summary>
    [Test]
    public void BoolParseTest()
    {
        string[] values = new string[] { "false", "true", "False", "True", "", "0", "1", "-1", "2" };
        bool[] parsable = new bool[] { true, true, true, true, false, false, false, false, false };
        bool[] parsedValue = new bool[] { false, true, false, true };

        for (int i = 0; i < values.Length; i++)
        {
            bool boolValue;

            bool boolParsed = bool.TryParse(values[i], out boolValue);
            Assert.IsTrue(boolParsed == parsable[i]);

            if (boolParsed)
            {
                Assert.IsTrue(boolValue == parsedValue[i]);
            }
        }
    }

    #endregion BoolParse Test
}

using System;
using UnityEngine;

namespace BGC.Utility.Math
{
    public static class ByteMath
    {
        public static byte RoundClamp(float value) =>
            (byte)Mathf.Clamp(Mathf.RoundToInt(value), byte.MinValue, byte.MaxValue);
        public static byte FloorClamp(float value) =>
            (byte)Mathf.Clamp(Mathf.FloorToInt(value), byte.MinValue, byte.MaxValue);

        public static byte RoundRepeat(float value) => (byte)Mathf.RoundToInt(value);
        public static byte FloorRepeat(float value) => (byte)Mathf.FloorToInt(value);
    }
}

using System;
using UnityEngine;

namespace BGC.Mathematics
{
    public static class ByteMath
    {
        public static byte RoundClamp(float value) =>
            (byte)GeneralMath.Clamp((int)Math.Round(value), byte.MinValue, byte.MaxValue);
        public static byte FloorClamp(float value) =>
            (byte)GeneralMath.Clamp((int)value, byte.MinValue, byte.MaxValue);

        public static byte RoundRepeat(float value) => (byte)((int)Math.Round(value) % byte.MaxValue);
        public static byte FloorRepeat(float value) => (byte)(((int)value) % byte.MaxValue);
    }
}

using System;
using UnityEngine;

namespace BGC.Mathematics
{
    public static class GeneralMath
    {
        public const double dPI = Math.PI;
        public const double d2PI = 2.0 * Math.PI;
        public const float fPI = (float)Math.PI;
        public const float f2PI = 2f * (float)Math.PI;

        public const double dDeg2Rad = dPI / 180.0;
        public const float fDeg2Rad = fPI / 180f;

        public const double dRad2Deg = 180.0 / dPI;
        public const float fRad2Deg = 180f / fPI;

        /// <summary>
        /// x mod m
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int Mod(int x, int m)
        {
            // https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
            return ((x % m) + m) % m;
        }

        /// <summary>
        /// x mod m
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }


        /// <summary>
        /// Hyperbolic Tangent in radian
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic tangent of the angle.</returns>
        // From Mathnet.Numerics
        public static double Tanh(double angle)
        {
            if (angle > 19.1)
            {
                return 1.0;
            }

            if (angle < -19.1)
            {
                return -1.0;
            }

            double e1 = Math.Exp(angle);
            double e2 = Math.Exp(-angle);
            return (e1 - e2) / (e1 + e2);
        }

        /// <summary>
        /// Hyperbolic Sine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic sine of the angle.</returns>
        // From Mathnet.Numerics
        public static double Sinh(double angle) => 0.5 * (Math.Exp(angle) - Math.Exp(-angle));

        /// <summary>
        /// Hyperbolic Cosine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic Cosine of the angle.</returns>
        // From Mathnet.Numerics
        public static double Cosh(double angle) => 0.5 * (Math.Exp(angle) + Math.Exp(-angle));

        /// <summary>
        /// Hyperbolic Tangent in radian
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic tangent of the angle.</returns>
        // From Mathnet.Numerics
        public static float Tanh(float angle)
        {
            if (angle > 19.1f)
            {
                return 1.0f;
            }

            if (angle < -19.1f)
            {
                return -1f;
            }

            float e1 = (float)Math.Exp(angle);
            float e2 = (float)Math.Exp(-angle);
            return (e1 - e2) / (e1 + e2);
        }

        /// <summary>
        /// Hyperbolic Sine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic sine of the angle.</returns>
        // From Mathnet.Numerics
        public static float Sinh(float angle) => (float)(Math.Exp(angle) - Math.Exp(-angle)) * 0.5f;

        /// <summary>
        /// Hyperbolic Cosine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic Cosine of the angle.</returns>
        // From Mathnet.Numerics
        public static float Cosh(float angle) => (float)(Math.Exp(angle) + Math.Exp(-angle)) * 0.5f;


        /// <summary>
        /// Find out whether the provided 32 bit integer is a perfect power of two.
        /// </summary>
        /// <param name="number">The number to very whether it's a power of two.</param>
        /// <returns>True if and only if it is a power of two.</returns>
        // From Mathnet.Numerics
        public static bool IsPowerOfTwo(this int number) =>
            number > 0 && (number & (number - 1)) == 0;

        public static int ToNextExponentOf2(this int x) => (int)Math.Ceiling(Math.Log(x) / Math.Log(2));

        public static int CeilingToPowerOfTwo(this int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        #region Clamp

        public static decimal Clamp(decimal value, decimal min, decimal max) =>
            Math.Max(min, Math.Min(max, value));

        public static double Clamp(double value, double min, double max) =>
            Math.Max(min, Math.Min(max, value));

        public static float Clamp(float value, float min, float max) =>
            Math.Max(min, Math.Min(max, value));

        public static long Clamp(long value, long min, long max) =>
            Math.Max(min, Math.Min(max, value));

        public static ulong Clamp(ulong value, ulong min, ulong max) =>
            Math.Max(min, Math.Min(max, value));

        public static int Clamp(int value, int min, int max) =>
            Math.Max(min, Math.Min(max, value));

        public static uint Clamp(uint value, uint min, uint max) =>
            Math.Max(min, Math.Min(max, value));

        public static short Clamp(short value, short min, short max) =>
            Math.Max(min, Math.Min(max, value));

        public static ushort Clamp(ushort value, ushort min, ushort max) =>
            Math.Max(min, Math.Min(max, value));

        public static byte Clamp(byte value, byte min, byte max) =>
            Math.Max(min, Math.Min(max, value));

        public static sbyte Clamp(sbyte value, sbyte min, sbyte max) =>
            Math.Max(min, Math.Min(max, value));

        #endregion Clamp
        #region Clamp01

        /// <summary>
        /// Clamps <paramref name="value"/> to range [0, 1]
        /// </summary>
        public static decimal Clamp01(decimal value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps <paramref name="value"/> to range [0, 1]
        /// </summary>
        public static double Clamp01(double value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps <paramref name="value"/> to range [0, 1]
        /// </summary>
        public static float Clamp01(float value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            return value;
        }

        #endregion Clamp01
        #region Lerp

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/>, clamps <paramref name="t"/> to range [0,1]
        /// </summary>
        public static decimal Lerp(decimal initial, decimal final, decimal t)
        {
            t = Clamp01(t);
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/> without clamping <paramref name="t"/> between 0 and 1
        /// </summary>
        public static decimal LerpUnclamped(decimal initial, decimal final, decimal t)
        {
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Calculates the lerp parameter of <paramref name="value"/> between bounds
        /// <paramref name="initial"/> and <paramref name="final"/>.
        /// </summary>
        public static decimal InverseLerp(decimal initial, decimal final, decimal value)
        {
            if (initial == final)
            {
                return 0;
            }

            return Clamp01((value - initial) / (final - initial));
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/>, clamps <paramref name="t"/> to range [0,1]
        /// </summary>
        public static double Lerp(double initial, double final, double t)
        {
            t = Clamp01(t);
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/> without clamping <paramref name="t"/> between 0 and 1
        /// </summary>
        public static double LerpUnclamped(double initial, double final, double t)
        {
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Calculates the lerp parameter of <paramref name="value"/> between bounds
        /// <paramref name="initial"/> and <paramref name="final"/>.
        /// </summary>
        public static double InverseLerp(double initial, double final, double value)
        {
            if (initial == final)
            {
                return 0;
            }

            return Clamp01((value - initial) / (final - initial));
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/>, clamps <paramref name="t"/> to range [0,1]
        /// </summary>
        public static float Lerp(float initial, float final, float t)
        {
            t = Clamp01(t);
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="initial"/> and <paramref name="final"/>
        /// by <paramref name="t"/> without clamping <paramref name="t"/> between 0 and 1
        /// </summary>
        public static float LerpUnclamped(float initial, float final, float t)
        {
            return initial * (1 - t) + final * t;
        }

        /// <summary>
        /// Calculates the lerp parameter of <paramref name="value"/> between bounds
        /// <paramref name="initial"/> and <paramref name="final"/>.
        /// </summary>
        public static float InverseLerp(float initial, float final, float value)
        {
            if (initial == final)
            {
                return 0;
            }

            return Clamp01((value - initial) / (final - initial));
        }

        #endregion Lerp
        #region Repeat

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        public static decimal Repeat(decimal value, decimal min, decimal max)
        {
            decimal scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        public static double Repeat(double value, double min, double max)
        {
            double scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        public static float Repeat(float value, float min, float max)
        {
            float scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        public static long Repeat(long value, long min, long max)
        {
            long scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        public static int Repeat(int value, int min, int max)
        {
            int scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        #endregion Repeat
        #region PingPong

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>], 
        /// forwards then backwards.
        /// </summary>
        public static decimal PingPong(decimal value, decimal min, decimal max)
        {
            value = Repeat(value - min, 0, (max - min) * 2);
            return max - Math.Abs(value - max + min);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [0,<paramref name="length"/>], 
        /// forwards then backwards.
        /// </summary>
        public static decimal PingPong(decimal value, decimal length)
        {
            value = Repeat(value, 0, length * 2);
            return length - Math.Abs(value - length);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>], 
        /// forwards then backwards.
        /// </summary>
        public static double PingPong(double value, double min, double max)
        {
            value = Repeat(value - min, 0, (max - min) * 2);
            return max - Math.Abs(value - max + min);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [0,<paramref name="length"/>], 
        /// forwards then backwards.
        /// </summary>
        public static double PingPong(double value, double length)
        {
            value = Repeat(value, 0, length * 2);
            return length - Math.Abs(value - length);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>], 
        /// forwards then backwards.
        /// </summary>
        public static float PingPong(float value, float min, float max)
        {
            value = Repeat(value - min, 0, (max - min) * 2);
            return max - Math.Abs(value - max + min);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [0,<paramref name="length"/>], 
        /// forwards then backwards.
        /// </summary>
        public static float PingPong(float value, float length)
        {
            value = Repeat(value, 0, length * 2);
            return length - Math.Abs(value - length);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>], 
        /// forwards then backwards.
        /// </summary>
        public static long PingPong(long value, long min, long max)
        {
            value = Repeat(value - min, 0, (max - min) * 2);
            return max - Math.Abs(value - max + min);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [0,<paramref name="length"/>], 
        /// forwards then backwards.
        /// </summary>
        public static long PingPong(long value, long length)
        {
            value = Repeat(value, 0, length * 2);
            return length - Math.Abs(value - length);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [<paramref name="min"/>,<paramref name="max"/>], 
        /// forwards then backwards.
        /// </summary>
        public static int PingPong(int value, int min, int max)
        {
            value = Repeat(value - min, 0, (max - min) * 2);
            return max - Math.Abs(value - max + min);
        }

        /// <summary>
        /// Loops <paramref name="value"/> in the range [0,<paramref name="length"/>], 
        /// forwards then backwards.
        /// </summary>
        public static int PingPong(int value, int length)
        {
            value = Repeat(value, 0, length * 2);
            return length - Math.Abs(value - length);
        }

        #endregion PingPong
        #region Approximately

        private const float FLOAT_MANTISSA_LOWER_BOUND = 1.1920929E-7f;
        private const float FLOAT_SMALLEST_NORMAL = 1.1754943508E-38f;
        private const float FLOAT_COMPARISON_MAX_FACTOR = 8 * FLOAT_MANTISSA_LOWER_BOUND;
        private const float FLOAT_COMPARISON_LOWER_BOUND = 8 * FLOAT_SMALLEST_NORMAL;

        public static bool Approximately(float a, float b)
        {
            return Math.Abs(b - a) <= Math.Max(
                FLOAT_COMPARISON_MAX_FACTOR * Math.Max(Math.Abs(a), Math.Abs(b)),
                FLOAT_COMPARISON_LOWER_BOUND);
        }

        private const double DOUBLE_MANTISSA_LOWER_BOUND = 2.220446E-16;
        private const double DOUBLE_SMALLEST_NORMAL = 2.2250738585072014E10 - 308;
        private const double DOUBLE_COMPARISON_MAX_FACTOR = 8 * DOUBLE_MANTISSA_LOWER_BOUND;
        private const double DOUBLE_COMPARISON_LOWER_BOUND = 8 * DOUBLE_SMALLEST_NORMAL;

        public static bool Approximately(double a, double b)
        {
            return Math.Abs(b - a) <= Math.Max(
                DOUBLE_COMPARISON_MAX_FACTOR * Math.Max(Math.Abs(a), Math.Abs(b)),
                DOUBLE_COMPARISON_LOWER_BOUND);
        }

        #endregion Approximately
    }
}
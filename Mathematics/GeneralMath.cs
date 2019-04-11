using System;
using UnityEngine;

namespace BGC.Mathematics
{
    public static class GeneralMath
    {
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

            float e1 = Mathf.Exp(angle);
            float e2 = Mathf.Exp(-angle);
            return (e1 - e2) / (e1 + e2);
        }

        /// <summary>
        /// Hyperbolic Sine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic sine of the angle.</returns>
        // From Mathnet.Numerics
        public static float Sinh(float angle) => (Mathf.Exp(angle) - Mathf.Exp(-angle)) / 2f;

        /// <summary>
        /// Hyperbolic Cosine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic Cosine of the angle.</returns>
        // From Mathnet.Numerics
        public static float Cosh(float angle) => (Mathf.Exp(angle) + Mathf.Exp(-angle)) / 2f;


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

        #endregion Lerp
        #region Repeat

        public static decimal Repeat(decimal value, decimal min, decimal max)
        {
            decimal scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        public static double Repeat(double value, double min, double max)
        {
            double scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        public static float Repeat(float value, float min, float max)
        {
            float scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        public static long Repeat(long value, long min, long max)
        {
            long scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        public static int Repeat(int value, int min, int max)
        {
            int scanningValue = (value - min) % (max - min);
            return scanningValue >= 0 ? scanningValue + min : scanningValue + max;
        }

        #endregion Clamp
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
        private const double DOUBLE_SMALLEST_NORMAL = 2.2250738585072014E10-308;
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
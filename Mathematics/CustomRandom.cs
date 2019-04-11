using System;

using static System.Math;

namespace BGC.Mathematics
{
    public static class CustomRandom
    {
        /// <summary>
        /// Lock used to protect multi-thread access to random
        /// </summary>
        static private object randLock = new object();

        static private Random _globalRand;
        static private Random GlobalRand
        {
            get
            {
                if (_globalRand == null)
                {
                    _globalRand = new Random();
                }

                return _globalRand;
            }
        }

        /// <summary>
        /// Returns a random double in the range [0.0 1.0)
        /// </summary>
        public static double NextDouble()
        {
            lock (randLock)
            {
                return GlobalRand.NextDouble();
            }
        }

        /// <summary>
        /// Returns a random float in the range [0f 1f].
        /// NOTE: 1 is inclusive, because of casting the double to a float rounding
        /// </summary>
        public static float NextFloat()
        {
            lock (randLock)
            {
                return GlobalRand.NextFloat();
            }
        }

        /// <summary>
        /// Returns a random integer
        /// </summary>
        public static int Next()
        {
            lock (randLock)
            {
                return GlobalRand.Next();
            }
        }

        /// <summary>
        /// Returns a random integer in the range [min max)
        /// </summary>
        public static int Next(int min, int max)
        {
            lock (randLock)
            {
                return GlobalRand.Next(min, max);
            }
        }

        /// <summary>
        /// Translates an evenly distributed random number in the range [0,1) into a Rayleigh-Distributed one.
        /// Technically in the range [0, 8.49151] due to the largest double less than one.
        /// About half the range of inputs results in a value less than 1, and 85% are less than 2.
        /// </summary>
        /// <param name="input">Random input value.  Should be in range [0 1]</param>
        public static double RayleighDistribution(double input) => Sqrt(-2.0 * Log(1.0 - input));

        /// <summary>
        /// Translates an evenly distributed random number in the range [0,1) into a Rayleigh-Distributed one.
        /// Technically in the range [0, 8.49151] due to the largest double less than one.
        /// About half the range of inputs results in a value less than 1, and 85% are less than 2.
        /// </summary>
        /// <param name="input">Random input value.  Should be in range [0 1]</param>
        public static float RayleighDistribution(float input) => (float)RayleighDistribution((double)input);

        /// <summary>
        /// Returns a normally distributed value with specified mean and deviation
        /// </summary>
        public static double NormalDistribution(double sigma, double mean, double input1, double input2) =>
            mean + sigma * NormalDistribution(input1, input2);

        /// <summary>
        /// Returns a normally distributed value with mean 0 and sigma 1
        /// https://en.wikipedia.org/wiki/Normal_distribution#Generating_values_from_normal_distribution
        /// </summary>
        public static double NormalDistribution(double input1, double input2) =>
            Sqrt(-2.0 * Log(input1)) * Cos(2.0 * PI * input2);

        /// <summary>
        /// Retrieve the next double and cast it to a float.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloat(this Random random) => (float)random.NextDouble();
    }
}

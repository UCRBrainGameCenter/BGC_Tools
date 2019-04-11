using System;
using System.Collections.Generic;

namespace BGC.Mathematics
{
    /// <summary>
    /// Handles Calculation of the Modified Bessel Function of the First Kind
    /// </summary>
    public static class Bessel
    {
        public class BesselCache
        {
            public float LastArgument { get; private set; } = float.NaN;

            private readonly Dictionary<int, double> besselCache = new Dictionary<int, double>();

            public BesselCache() { }

            public void ClearCache(float newArgument)
            {
                besselCache.Clear();
                LastArgument = newArgument;
            }

            public double this[int order]
            {
                get => besselCache[order];
                set => besselCache[order] = value;
            }

            public bool Contains(int order) => besselCache.ContainsKey(order);
        }

        public class DoubleBesselCache
        {
            public double LastArgument { get; private set; } = double.NaN;

            private readonly Dictionary<int, double> besselCache = new Dictionary<int, double>();

            public DoubleBesselCache() { }

            public void ClearCache(double newArgument)
            {
                besselCache.Clear();
                LastArgument = newArgument;
            }

            public double this[int order]
            {
                get => besselCache[order];
                set => besselCache[order] = value;
            }

            public bool Contains(int order) => besselCache.ContainsKey(order);
        }

        /// <summary>
        /// Returns the modified Bessel function I_\nu(x) for integer order \nu >= 0 and real argument
        /// Caches all return values for the last argument used.
        /// Flushes cache when a new argument is used.
        /// 
        /// Adapted from:
        /// "Numerical Recipes in C", Second Edition, Press, WH et al.,
        ///   Cambridge University Press, page 237
        /// </summary>
        /// <param name="order">non-negative integer order</param>
        /// <param name="argument">real-valued argument</param>
        /// <returns></returns>
        public static float Bessi(
            int order,
            float argument,
            BesselCache cache = null)
        {
            if (cache != null)
            {
                if (argument != cache.LastArgument)
                {
                    cache.ClearCache(argument);
                    cache[0] = Bessi0(argument);
                }

                if (cache.Contains(order))
                {
                    return (float)cache[order];
                }
            }

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException($"Bessel order must be greater than or equal to 0.  Received: {order}");
            }

            //Cast argument up to a double
            double x = argument;

            double besselValue;

            if (order == 0)
            {
                besselValue = Bessi0(x);
            }
            else if (order == 1)
            {
                besselValue = Bessi1(x);
            }
            else if (x == 0.0)
            {
                return 0f;
            }
            else
            {
                const double ACC = 40.0;
                const double BIGNO = 1.0e10;
                const double BIGNI = 1.0e-10;

                double bim;
                double bi = 1.0;
                double tox = 2.0 / Math.Abs(x);
                double bip = 0.0;
                double ans = 0.0;

                for (int j = 2 * (order + (int)Math.Sqrt(ACC * order)); j > 0; j--)
                {
                    bim = bip + j * tox * bi;
                    bip = bi;
                    bi = bim;
                    if (Math.Abs(bi) > BIGNO)
                    {
                        ans *= BIGNI;
                        bi *= BIGNI;
                        bip *= BIGNI;
                    }

                    if (j == order)
                    {
                        ans = bip;
                    }
                }

                //Normalize with the 0th order term
                if (cache != null)
                {
                    ans *= cache[0] / bi;
                }
                else
                {
                    ans *= Bessi0(argument) / bi;
                }
                besselValue = x < 0.0 && order % 2 == 1 ? -ans : ans;
            }

            if (cache != null)
            {
                cache[order] = besselValue;
            }

            return (float)besselValue;
        }

        /// <summary>
        /// Returns the modified Bessel function I_\nu(x) for integer order \nu >= 0 and real argument
        /// Caches all return values for the last argument used.
        /// Flushes cache when a new argument is used.
        /// 
        /// Adapted from:
        /// "Numerical Recipes in C", Second Edition, Press, WH et al.,
        ///   Cambridge University Press, page 237
        /// </summary>
        /// <param name="order">non-negative integer order</param>
        /// <param name="argument">real-valued argument</param>
        /// <returns></returns>
        public static double Bessi(
            int order,
            double argument,
            DoubleBesselCache cache = null)
        {
            if (cache != null)
            {
                if (argument != cache.LastArgument)
                {
                    cache.ClearCache(argument);
                    cache[0] = Bessi0(argument);
                }

                if (cache.Contains(order))
                {
                    return cache[order];
                }
            }

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException($"Bessel order must be greater than or equal to 0.  Received: {order}");
            }

            //Cast argument up to a double
            double x = argument;

            double besselValue;

            if (order == 0)
            {
                besselValue = Bessi0(x);
            }
            else if (order == 1)
            {
                besselValue = Bessi1(x);
            }
            else if (x == 0.0)
            {
                return 0.0;
            }
            else
            {
                const double ACC = 40.0;
                const double BIGNO = 1.0e10;
                const double BIGNI = 1.0e-10;

                double bim;
                double bi = 1.0;
                double tox = 2.0 / Math.Abs(x);
                double bip = 0.0;
                double ans = 0.0;

                for (int j = 2 * (order + (int)Math.Sqrt(ACC * order)); j > 0; j--)
                {
                    bim = bip + j * tox * bi;
                    bip = bi;
                    bi = bim;
                    if (Math.Abs(bi) > BIGNO)
                    {
                        ans *= BIGNI;
                        bi *= BIGNI;
                        bip *= BIGNI;
                    }

                    if (j == order)
                    {
                        ans = bip;
                    }
                }

                //Normalize with the 0th order term
                if (cache != null)
                {
                    ans *= cache[0] / bi;
                }
                else
                {
                    ans *= Bessi0(argument) / bi;
                }
                besselValue = x < 0.0 && order % 2 == 1 ? -ans : ans;
            }

            if (cache != null)
            {
                cache[order] = besselValue;
            }

            return besselValue;
        }

        /// <summary>
        /// Evaluate modified Bessel function In(x) and order n=0.
        /// 
        /// Adapted from:
        /// "Numerical Recipes in C", Second Edition, Press, WH et al.,
        ///   Cambridge University Press, page 237
        /// </summary>
        /// <param name="x">Modified Bessel Function Argument</param>
        /// <returns>Modified Bessel Function of order n=0 at Argument</returns>
        static double Bessi0(double x)
        {
            double ax, ans;
            double y;

            if ((ax = Math.Abs(x)) < 3.75)
            {
                y = x / 3.75;
                y *= y;
                ans = 1.0 + y * (3.5156229 + y * (3.0899424 + y * (1.2067492
                   + y * (0.2659732 + y * (0.360768e-1 + y * 0.45813e-2)))));
            }
            else
            {
                y = 3.75 / ax;
                ans = (Math.Exp(ax) / Math.Sqrt(ax)) * (0.39894228 + y * (0.1328592e-1
                   + y * (0.225319e-2 + y * (-0.157565e-2 + y * (0.916281e-2
                   + y * (-0.2057706e-1 + y * (0.2635537e-1 + y * (-0.1647633e-1
                   + y * 0.392377e-2))))))));
            }

            return ans;
        }

        /// <summary>
        /// Evaluate modified Bessel function In(x) and order n=1.
        /// 
        /// Adapted from:
        /// "Numerical Recipes in C", Second Edition, Press, WH et al.,
        ///   Cambridge University Press, page 237
        /// </summary>
        /// <param name="x">Modified Bessel Function Argument</param>
        /// <returns>Modified Bessel Function of order n=1 at Argument</returns>
        static double Bessi1(double x)
        {
            double ax, ans;
            double y;

            if ((ax = Math.Abs(x)) < 3.75)
            {
                y = x / 3.75;
                y *= y;
                ans = ax * (0.5 + y * (0.87890594 + y * (0.51498869 + y * (0.15084934
                   + y * (0.2658733e-1 + y * (0.301532e-2 + y * 0.32411e-3))))));
            }
            else
            {
                y = 3.75 / ax;
                ans = 0.2282967e-1 + y * (-0.2895312e-1 + y * (0.1787654e-1
                   - y * 0.420059e-2));
                ans = 0.39894228 + y * (-0.3988024e-1 + y * (-0.362018e-2
                   + y * (0.163801e-2 + y * (-0.1031555e-1 + y * ans))));
                ans *= (Math.Exp(ax) / Math.Sqrt(ax));
            }

            return x < 0.0 ? -ans : ans;
        }
    }
}

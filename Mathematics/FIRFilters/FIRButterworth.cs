using System;
using BGC.Mathematics;
using BGC.Audio;

using static System.Math;
using static BGC.Mathematics.GeneralMath;

namespace BGC.Mathematics.FIRFilters
{
    public static class FIRButterworth
    {
        public static double[] BandpassCoefficients(
            int order,
            double f1,
            double f2,
            double samplingRate)
        {
            //Technically this is half the associated wavenumbers
            double w1 = f1 / samplingRate;
            double w2 = f2 / samplingRate;

            int halfLength = order / 2;

            //output
            double[] coefficients = new double[order + 1];

            int typeOffset = ((order % 2) == 0) ? 0 : 1;

            double cosineArg = 2 * PI / order;
            for (int i = 0; i <= halfLength; i++)
            {
                int coeffArg = 2 * (halfLength - i) + typeOffset;

                coefficients[i] = coefficients[order - i] =
                    (w2 * Sinc(coeffArg * w2) - w1 * Sinc(coeffArg * w1)) *
                    (1.0 - Cos(i * cosineArg));
            }

            //Scale the coefficients
            double argument = -PI * (w1 + w2);

            double realSum = 0.0;
            double imagSum = 0.0;

            for (int i = 0; i < coefficients.Length; i++)
            {
                //h[x] * Exp(-pi i f0 x)
                double temp = argument * i;

                realSum += coefficients[i] * Cos(temp);
                imagSum += coefficients[i] * Sin(temp);
            }

            double factor = 1.0 / Sqrt(realSum * realSum + imagSum * imagSum);

            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] *= factor;
            }

            return coefficients;
        }
    }
}

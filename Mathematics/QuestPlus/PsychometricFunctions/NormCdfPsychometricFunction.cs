using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Mathematics.QuestPlus.PsychometricFunctions
{
    /// <summary>
    /// Cumulative normal psychometric function used for two-alternative
    /// comparison experiments (e.g., "which is greater" tasks).
    ///
    /// Parameter order: [mean, sd, lower_asymptote, lapse_rate].
    /// Stimulus order: [intensity].
    ///
    /// Formula: p = gamma + (1 - gamma - delta) * Phi((x - mean) / sd)
    /// where Phi is the standard normal CDF.
    /// </summary>
    public sealed class NormCdfPsychometricFunction : IPsychometricFunction
    {
        public string TypeId => "norm_cdf";
        public int StimDimensionCount => 1;
        public int ParamDimensionCount => 4;
        public int OutcomeCount => 2;

        public IReadOnlyList<string> StimDimensionNames { get; } = new[] { "intensity" };
        public IReadOnlyList<string> ParamDimensionNames { get; } =
            new[] { "mean", "sd", "lower_asymptote", "lapse_rate" };

        public double[] Evaluate(double[] stim, double[] paramVals, double[] destination = null)
        {
            double x = stim[0];
            double mean = paramVals[0];
            double sd = paramVals[1];
            double gamma = paramVals[2];
            double delta = paramVals[3];

            double z = (x - mean) / sd;
            double phi = StandardNormalCdf(z);
            double p = gamma + (1.0 - gamma - delta) * phi;

            if (p < 0.0) p = 0.0;
            else if (p > 1.0) p = 1.0;

            double[] result = destination ?? new double[2];
            result[0] = p;
            result[1] = 1.0 - p;
            return result;
        }

        /// <summary>
        /// Standard normal CDF via erf. Accurate to roughly 1e-7.
        /// </summary>
        internal static double StandardNormalCdf(double z)
        {
            return 0.5 * (1.0 + Erf(z / Math.Sqrt(2.0)));
        }

        /// <summary>
        /// Abramowitz & Stegun 7.1.26 approximation of the error function.
        /// </summary>
        internal static double Erf(double x)
        {
            // Constants
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        public JsonObject SerializeConfig() => new JsonObject();

        public static NormCdfPsychometricFunction FromConfig(JsonObject _) => new NormCdfPsychometricFunction();
    }
}

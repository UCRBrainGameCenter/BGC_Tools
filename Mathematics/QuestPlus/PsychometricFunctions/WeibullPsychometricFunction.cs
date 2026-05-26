using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Mathematics.QuestPlus.PsychometricFunctions
{
    /// <summary>
    /// Stimulus scaling used by Weibull and CSF psychometric functions.
    /// </summary>
    public enum WeibullScale
    {
        /// <summary>Linear stimulus magnitude (e.g., raw contrast).</summary>
        Linear,

        /// <summary>Log10 of the stimulus magnitude.</summary>
        Log10,

        /// <summary>Decibels (20 * log10).</summary>
        DB,
    }

    /// <summary>
    /// Weibull psychometric function with threshold, slope, lower asymptote,
    /// and lapse rate parameters. Returns [p(outcome0), p(outcome1)] where
    /// outcome0 is the "success" / "correct" outcome.
    ///
    /// Parametrizations (matching the questplus Python reference):
    ///   linear: p = 1 - delta - (1 - gamma - delta) * exp(-(x/t)^beta)
    ///   log10:  p = 1 - delta - (1 - gamma - delta) * exp(-10^(beta*(x - t)))
    ///   dB:     p = 1 - delta - (1 - gamma - delta) * exp(-10^(beta*(x - t)/20))
    ///
    /// Parameter order: [threshold, slope, lower_asymptote, lapse_rate].
    /// Stimulus order: [intensity].
    /// </summary>
    public sealed class WeibullPsychometricFunction : IPsychometricFunction
    {
        public string TypeId => "weibull";
        public int StimDimensionCount => 1;
        public int ParamDimensionCount => 4;
        public int OutcomeCount => 2;

        public WeibullScale Scale { get; }

        public IReadOnlyList<string> StimDimensionNames { get; } = new[] { "intensity" };
        public IReadOnlyList<string> ParamDimensionNames { get; } =
            new[] { "threshold", "slope", "lower_asymptote", "lapse_rate" };

        public WeibullPsychometricFunction(WeibullScale scale = WeibullScale.Log10)
        {
            Scale = scale;
        }

        public double[] Evaluate(double[] stim, double[] paramVals, double[] destination = null)
        {
            double x = stim[0];
            double t = paramVals[0];
            double beta = paramVals[1];
            double gamma = paramVals[2];
            double delta = paramVals[3];

            double p;
            switch (Scale)
            {
                case WeibullScale.Linear:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(x / t, beta));
                    break;
                case WeibullScale.Log10:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(10.0, beta * (x - t)));
                    break;
                case WeibullScale.DB:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(10.0, beta * (x - t) / 20.0));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown Weibull scale: {Scale}");
            }

            // Clamp to valid probability range to avoid tiny negatives from
            // floating point at extreme parameter combinations.
            if (p < 0.0) p = 0.0;
            else if (p > 1.0) p = 1.0;

            double[] result = destination ?? new double[2];
            result[0] = p;
            result[1] = 1.0 - p;
            return result;
        }

        public JsonObject SerializeConfig() => new JsonObject
        {
            ["scale"] = Scale.ToString(),
        };

        public static WeibullPsychometricFunction FromConfig(JsonObject config)
        {
            string scaleStr = config["scale"].AsString;
            if (!Enum.TryParse(scaleStr, out WeibullScale scale))
            {
                throw new ArgumentException($"Unknown Weibull scale: {scaleStr}");
            }
            return new WeibullPsychometricFunction(scale);
        }
    }
}

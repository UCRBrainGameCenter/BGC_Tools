using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Mathematics.QuestPlus.PsychometricFunctions
{
    /// <summary>
    /// Contrast sensitivity function psychometric function (Watson 2017,
    /// "Pyramid of Visibility" model).
    ///
    /// Stimulus order: [contrast, spatial_freq, temporal_freq].
    /// Parameter order: [min_thresh, c0, cf, cw, slope, lower_asymptote, lapse_rate].
    ///
    /// Threshold is defined as t = max(min_thresh, c0 + cf*spatial_freq + cw*temporal_freq).
    /// The psychometric function around this threshold is a Weibull whose scale
    /// is chosen via the <see cref="WeibullScale"/> setting.
    /// </summary>
    public sealed class CsfPsychometricFunction : IPsychometricFunction
    {
        public string TypeId => "csf";
        public int StimDimensionCount => 3;
        public int ParamDimensionCount => 7;
        public int OutcomeCount => 2;

        public WeibullScale Scale { get; }

        public IReadOnlyList<string> StimDimensionNames { get; } =
            new[] { "contrast", "spatial_freq", "temporal_freq" };
        public IReadOnlyList<string> ParamDimensionNames { get; } =
            new[] { "min_thresh", "c0", "cf", "cw", "slope", "lower_asymptote", "lapse_rate" };

        public CsfPsychometricFunction(WeibullScale scale = WeibullScale.Log10)
        {
            Scale = scale;
        }

        public double[] Evaluate(double[] stim, double[] paramVals, double[] destination = null)
        {
            double contrast = stim[0];
            double spatialFreq = stim[1];
            double temporalFreq = stim[2];

            double minT = paramVals[0];
            double c0 = paramVals[1];
            double cf = paramVals[2];
            double cw = paramVals[3];
            double beta = paramVals[4];
            double gamma = paramVals[5];
            double delta = paramVals[6];

            double t = Math.Max(minT, c0 + cf * spatialFreq + cw * temporalFreq);

            double p;
            switch (Scale)
            {
                case WeibullScale.Linear:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(contrast / t, beta));
                    break;
                case WeibullScale.Log10:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(10.0, beta * (contrast - t)));
                    break;
                case WeibullScale.DB:
                    p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(10.0, beta * (contrast - t) / 20.0));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown CSF scale: {Scale}");
            }

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

        public static CsfPsychometricFunction FromConfig(JsonObject config)
        {
            string scaleStr = config["scale"].AsString;
            if (!Enum.TryParse(scaleStr, out WeibullScale scale))
            {
                throw new ArgumentException($"Unknown CSF scale: {scaleStr}");
            }
            return new CsfPsychometricFunction(scale);
        }
    }
}

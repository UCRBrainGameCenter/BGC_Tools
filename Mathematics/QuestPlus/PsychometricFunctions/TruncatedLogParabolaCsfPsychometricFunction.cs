using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Mathematics.QuestPlus.PsychometricFunctions
{
    /// <summary>
    /// Contrast sensitivity function based on the truncated log-parabola
    /// parametrization (Lesmes, Lu, Baek &amp; Albright 2010 — the qCSF
    /// method). Used to estimate a full CSF curve from a single QUEST+
    /// run that jointly adapts contrast and spatial frequency.
    ///
    /// <para>
    /// Stim domain (in order): contrast, spatial_freq.
    /// Param domain (in order): peakSensitivity, peakFrequency, bandwidth,
    /// lowFreqTruncation, slope, lowerAsymptote, lapseRate.
    /// </para>
    ///
    /// <para>
    /// Contrast is in dB internally — the math is log-space, and sensitivity /
    /// threshold are in dB (20·log10). The contrast threshold at a given spatial
    /// frequency f is:
    ///
    /// <code>
    ///   logThresh_dB(f) = -peakSensitivity_dB
    ///                   + 20 * log10(2) * ((log10(f) - log10(f_peak)) / (bandwidth * log10(2) / 2))^2
    /// </code>
    ///
    /// where <c>bandwidth</c> is the full width at half-maximum of the log-parabola
    /// in octaves: at |log2(f/f_peak)| = bandwidth/2 the sensitivity falls by
    /// 20·log10(2) dB (a factor of two). It is
    /// truncated on the low-frequency side: for f &lt; f_peak the parabola
    /// is replaced by the constant -peakSensitivity_dB + lowFreqTruncation
    /// when that constant exceeds the parabola value (i.e. the low-frequency
    /// shoulder of the CSF is bounded above the parabola).
    /// </para>
    ///
    /// <para>
    /// Around that threshold the psychometric function is a dB-scale Weibull:
    ///
    /// <code>
    ///   p(correct) = 1 - lapse - (1 - guess - lapse) * exp(-10^(slope * (c - logThresh(f)) / 20))
    /// </code>
    /// </para>
    /// </summary>
    public sealed class TruncatedLogParabolaCsfPsychometricFunction : IPsychometricFunction
    {
        public string TypeId => "truncated_log_parabola_csf";
        public int StimDimensionCount => 2;
        public int ParamDimensionCount => 7;
        public int OutcomeCount => 2;

        public IReadOnlyList<string> StimDimensionNames { get; } = new[] { "contrast", "spatial_freq" };
        public IReadOnlyList<string> ParamDimensionNames { get; } = new[]
        {
            "peak_sensitivity",       // dB sensitivity at the CSF peak
            "peak_frequency",         // spatial frequency (cycles/degree) of the peak
            "bandwidth",              // full bandwidth at half-max sensitivity, in octaves
            "low_freq_truncation",    // dB drop allowed below peak on the low-freq side (>= 0)
            "slope",                  // Weibull slope (dB)
            "lower_asymptote",        // guess rate
            "lapse_rate",             // lapse rate
        };

        public double[] Evaluate(double[] stim, double[] paramVals, double[] destination = null)
        {
            double contrastDb = stim[0];
            double spatialFreq = stim[1];

            double peakSens = paramVals[0];
            double peakFreq = paramVals[1];
            double bandwidth = paramVals[2];
            double lowFreqTrunc = paramVals[3];
            double slope = paramVals[4];
            double gamma = paramVals[5];
            double delta = paramVals[6];

            double thresholdDb = ComputeLogThresholdDb(spatialFreq, peakSens, peakFreq, bandwidth, lowFreqTrunc);

            double exponent = slope * (contrastDb - thresholdDb) / 20.0;
            double p = 1.0 - delta - (1.0 - gamma - delta) * Math.Exp(-Math.Pow(10.0, exponent));

            if (p < 0.0) p = 0.0;
            else if (p > 1.0) p = 1.0;

            double[] result = destination ?? new double[2];
            result[0] = p;
            result[1] = 1.0 - p;
            return result;
        }

        /// <summary>
        /// Compute the dB contrast threshold at the given spatial frequency
        /// using the truncated log-parabola CSF model. Threshold is in dB
        /// contrast (negative numbers mean sub-zero-dB contrast is needed).
        /// </summary>
        internal static double ComputeLogThresholdDb(
            double spatialFreq, double peakSens, double peakFreq, double bandwidth, double lowFreqTrunc)
        {
            // Guard against pathological inputs that would make log10 blow up.
            if (spatialFreq <= 0 || peakFreq <= 0 || bandwidth <= 0)
            {
                return -peakSens;
            }

            double logF = Math.Log10(spatialFreq);
            double logFPeak = Math.Log10(peakFreq);
            // Log-parabola sensitivity drop from the peak, in dB. "bandwidth" is
            // the full width at half-maximum in OCTAVES: at
            // |log2(f / f_peak)| = bandwidth / 2 the sensitivity falls by
            // 20 * log10(2) dB (a factor of two). The log10(2) factor converts the
            // octave half-bandwidth into the log10 (decade) units of
            // (logF - logFPeak). This is the truncated log-parabola (qCSF)
            // parametrization, expressed in dB sensitivity.
            double normalizedDelta = (logF - logFPeak) / (bandwidth * Math.Log10(2.0) / 2.0);
            double parabolaDrop = normalizedDelta * normalizedDelta * 20.0 * Math.Log10(2.0);

            // Sensitivity in dB at this frequency (parabola form, no truncation).
            double sensitivityDb = peakSens - parabolaDrop;

            // Low-frequency truncation: on the low-frequency side, sensitivity
            // doesn't drop below (peakSens - lowFreqTrunc). This produces the
            // characteristic flat / shallow low-frequency shoulder of the CSF.
            if (spatialFreq < peakFreq)
            {
                double truncatedSens = peakSens - lowFreqTrunc;
                if (sensitivityDb < truncatedSens)
                {
                    sensitivityDb = truncatedSens;
                }
            }

            // Threshold (dB) = -Sensitivity (dB).
            return -sensitivityDb;
        }

        public JsonObject SerializeConfig() => new JsonObject
        {
            ["type"] = TypeId,
        };

        public static TruncatedLogParabolaCsfPsychometricFunction FromConfig(JsonObject _)
            => new TruncatedLogParabolaCsfPsychometricFunction();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Audiometry
{
    /// <summary>
    /// A collection of some common procedures related to level scaling of Audio
    /// </summary>
    public static class LevelRegulation
    {
        public enum Scheme
        {
            RMS = 0,
            Peak,
            RMSAssigned,
            MAX
        }

        public const double dbSafetyLimit = 90.0;

        public static void GetAmplitudeFactors(
            double dbHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out double factorL,
            out double factorR,
            AudiometricCalibration.Source source = AudiometricCalibration.Source.Custom,
            bool safetyLimit = true)
        {
            if (safetyLimit && dbHL > dbSafetyLimit)
            {
                throw new StreamCompositionException(
                    $"Tried to exceed safety limit of 90dB HL without disengaging safety. " +
                    $"Requested Level: {dbHL} dB");
            }

            // Look up the SPL level for the given HL level
            double dbSPL = AudiometricCalibration.GetLevelSPL(
                calibrationSource: source,
                frequency: calibrationFrequency,
                levelHL: dbHL);

            factorL = AudiometricCalibration.GetLevelRMS(
                levelSPL: dbSPL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                channel: AudioChannel.Left,
                source: source);

            factorR = AudiometricCalibration.GetLevelRMS(
                levelSPL: dbSPL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                channel: AudioChannel.Right,
                source: source);
        }

        /// <summary>
        /// As above, with a request above 90 dB HL refused, clamped to it or allowed per
        /// <paramref name="mode"/> (and reported through <see cref="SafetyLimit.EventReporter"/>).
        /// </summary>
        public static void GetAmplitudeFactors(
            double dbHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out double factorL,
            out double factorR,
            AudiometricCalibration.Source source,
            SafetyLimitMode mode)
        {
            dbHL = SafetyLimit.Resolve(mode, "Request", "dB HL", dbSafetyLimit, 0.0, dbHL, dbHL, -1,
                $"Requested Level: {dbHL} dB");

            GetAmplitudeFactors(
                dbHL: dbHL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                factorL: out factorL,
                factorR: out factorR,
                source: source,
                safetyLimit: false);
        }

        public static void GetRMSScalingFactors(
            IBGCStream stream,
            double presentationLevelHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out double scalingFactorL,
            out double scalingFactorR,
            AudiometricCalibration.Source source = AudiometricCalibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetRMSScalingFactors(
                stream: stream,
                presentationLevelHL: presentationLevelHL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                scalingFactorL: out scalingFactorL,
                scalingFactorR: out scalingFactorR,
                source: source,
                mode: SafetyLimit.ModeFor(safetyLimit));
        }

        /// <summary>
        /// The scaling factors that play <paramref name="stream"/> at <paramref name="presentationLevelHL"/>,
        /// with the 90 dB HL safety limit applied per <paramref name="mode"/>: a request above it, or
        /// samples that would deliver more than it (+0.1 dB) at that request, are refused, turned down
        /// to the limit, or allowed, and reported either way, as the dB SPL regulators do
        /// (Normalization.GetRMSScalingFactors). The delivered level is judged against each ear's
        /// calibrated 90 dB HL output, not by adding the samples' excess to the request: a profile's
        /// attenuation is measured at several levels, so 1 dB of gain need not be 1 dB HL.
        /// </summary>
        public static void GetRMSScalingFactors(
            IBGCStream stream,
            double presentationLevelHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out double scalingFactorL,
            out double scalingFactorR,
            AudiometricCalibration.Source source,
            SafetyLimitMode mode)
        {
            //The request itself: refused, clamped to the limit, or allowed. Allowed above the limit,
            //it is reported below, once the level its samples deliver is known.
            bool allowedAboveLimit = mode == SafetyLimitMode.Off && presentationLevelHL > dbSafetyLimit;
            double level = allowedAboveLimit ? presentationLevelHL :
                SafetyLimit.Resolve(mode, "Request", "dB HL", dbSafetyLimit, 0.0,
                    presentationLevelHL, presentationLevelHL, -1, $"Requested Level: {presentationLevelHL} dB");

            GetAmplitudeFactors(
                dbHL: level,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: false);

            IEnumerable<double> channelRMS = stream.GetChannelRMS();
            bool rmsMeasured = false;

            if (channelRMS.Any(double.IsNaN))
            {
                if (stream.ChannelSamples == int.MaxValue)
                {
                    throw new StreamCompositionException("Unable to calculate the RMS of an infinite, unknowable stream.  Try truncating.");
                }
                else
                {
                    channelRMS = stream.CalculateRMS();
                    rmsMeasured = true;

                    if (channelRMS.All(double.IsNaN))
                    {
                        throw new StreamCompositionException("Unable to calculate the RMS of stream.");
                    }

                    if (channelRMS.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
                    {
                        //One channel's samples aren't numbers: dropping it would play them (PART-984)
                        throw new StreamCompositionException(
                            "Can't regulate a stimulus whose samples aren't finite numbers (NaN or infinity).");
                    }
                }
            }

            double maxRMS = channelRMS.Where(x => !double.IsNaN(x)).Max();

            Normalization.CheckRegulatable(maxRMS, level, "dB HL");

            //The claimed RMS may not match the samples, so check the level actually delivered.
            //Samples that would play above the limit are refused, turned down to it, or allowed.
            double attenuation = 1.0;
            double sampleLevel = level;
            bool measured = !rmsMeasured && SafetyLimit.WantsMeasurement(mode) &&
                Normalization.TryMeasureDeliveredLevel(stream, level, maxRMS, "dB HL", -1, out sampleLevel, out int _);
            double excessDB = measured ? sampleLevel - level : 0.0;

            if (allowedAboveLimit || measured)
            {
                //Each ear's output against that ear's output at the limit, in dB re the limit
                GetAmplitudeFactors(
                    dbHL: dbSafetyLimit,
                    calibrationSet: calibrationSet,
                    calibrationFrequency: calibrationFrequency,
                    factorL: out double limitFactorL,
                    factorR: out double limitFactorR,
                    source: source,
                    safetyLimit: false);

                (double deliveredL, double deliveredR) = DeliveredLevelsAtLimit(
                    levelFactorL, levelFactorR, limitFactorL, limitFactorR, excessDB);
                double deliveredLevel = Math.Max(deliveredL, deliveredR);
                int channel = Math.Abs(deliveredL - deliveredR) < 0.01 ? -1 : deliveredL > deliveredR ? 0 : 1;
                string detail = $"Requested Level: {level} dB HL, but the stimulus would deliver " +
                    $"{deliveredLevel:0.0} dB HL by the calibration at the limit (its samples are " +
                    $"{excessDB:+0.0;-0.0} dB from its claimed RMS)";

                if (allowedAboveLimit)
                {
                    //One event per stimulus: the request, with the level its samples deliver
                    SafetyLimit.Report(SafetyLimitMode.Off, "Request", "dB HL", dbSafetyLimit, presentationLevelHL,
                        deliveredLevel, deliveredLevel, channel, Math.Abs(deliveredLevel - level) >= 0.05 ?
                            detail : $"Requested Level: {presentationLevelHL} dB");
                }
                else
                {
                    double appliedLevel = SafetyLimit.Resolve(
                        mode: mode,
                        check: "Delivered",
                        unit: "dB HL",
                        limit: dbSafetyLimit,
                        tolerance: Normalization.deliveredLevelTolerance,
                        requestedLevel: level,
                        deliveredLevel: deliveredLevel,
                        channel: channel,
                        detail: detail);

                    if (appliedLevel < deliveredLevel)
                    {
                        attenuation = Math.Pow(10.0, (appliedLevel - deliveredLevel) / 20.0);
                    }
                }
            }

            scalingFactorL = attenuation * levelFactorL / maxRMS;
            scalingFactorR = attenuation * levelFactorR / maxRMS;

            //Protect against some NaN Poisoning
            if (double.IsNaN(scalingFactorL) || double.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1.0;
            }

            if (double.IsNaN(scalingFactorR) || double.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1.0;
            }
        }

        /// <summary>
        /// Each ear's delivered level in dB HL, judged on the calibration at the limit: 90 dB HL
        /// plus how far that ear's output (its amplitude factor at the request, times the samples'
        /// <paramref name="excessDB"/> over their claim) is above its output at 90 dB HL
        /// (<paramref name="limitFactorL"/>, <paramref name="limitFactorR"/>). For a calibration whose
        /// attenuation changes 1 dB per dB this is the request plus the excess.
        /// </summary>
        public static (double left, double right) DeliveredLevelsAtLimit(
            double levelFactorL,
            double levelFactorR,
            double limitFactorL,
            double limitFactorR,
            double excessDB) =>
            (dbSafetyLimit + excessDB + 20.0 * Math.Log10(levelFactorL / limitFactorL),
             dbSafetyLimit + excessDB + 20.0 * Math.Log10(levelFactorR / limitFactorR));

        public static void GetRMSScalingFactors(
            float[] stereoSamples,
            double presentationLevelHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out float scalingFactorL,
            out float scalingFactorR,
            AudiometricCalibration.Source source = AudiometricCalibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbHL: presentationLevelHL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            double[] sampleSquaredSum = new double[2];
            int sampleCount = stereoSamples.Length / 2;

            for (int i = 0; i < sampleCount; i++)
            {
                sampleSquaredSum[0] += stereoSamples[2 * i] * stereoSamples[2 * i];
                sampleSquaredSum[1] += stereoSamples[2 * i + 1] * stereoSamples[2 * i + 1];
            }

            sampleSquaredSum[0] = Math.Sqrt(sampleSquaredSum[0] / sampleCount);
            sampleSquaredSum[1] = Math.Sqrt(sampleSquaredSum[1] / sampleCount);

            double maxRMS = Math.Max(sampleSquaredSum[0], sampleSquaredSum[1]);

            scalingFactorL = (float)(levelFactorL / maxRMS);
            scalingFactorR = (float)(levelFactorR / maxRMS);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalingFactorL) || float.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1f;
            }

            if (float.IsNaN(scalingFactorR) || float.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1f;
            }
        }

        public static void GetMonoRMSScalingFactors(
            float[] monoSamples,
            double presentationLevelHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            out double scalingFactorL,
            out double scalingFactorR,
            AudiometricCalibration.Source source = AudiometricCalibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbHL: presentationLevelHL,
                calibrationSet: calibrationSet,
                calibrationFrequency: calibrationFrequency,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            double sampleSquaredSum = 0.0;

            for (int i = 0; i < monoSamples.Length; i++)
            {
                sampleSquaredSum += monoSamples[i] * monoSamples[i];
            }

            sampleSquaredSum = Math.Sqrt(sampleSquaredSum / monoSamples.Length);

            scalingFactorL = levelFactorL / sampleSquaredSum;
            scalingFactorR = levelFactorR / sampleSquaredSum;

            //Protect against some NaN Poisoning
            if (double.IsNaN(scalingFactorL) || double.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1.0;
            }

            if (double.IsNaN(scalingFactorR) || double.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1.0;
            }
        }

        public static void NormalizeRMSMono(float[] samples)
        {
            double squaredSum = 0.0;
            int sampleCount = samples.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                squaredSum += samples[i] * samples[i];
            }

            squaredSum = Math.Sqrt(squaredSum / sampleCount);

            float scalingFactor = (float)(1.0 / squaredSum);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalingFactor) || float.IsInfinity(scalingFactor))
            {
                scalingFactor = 1f;
            }

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        public static void NormalizeRMSStereo(float[] samples)
        {
            double squaredSumL = 0.0;
            double squaredSumR = 0.0;
            int sampleCount = samples.Length / 2;

            for (int i = 0; i < sampleCount; i += 2)
            {
                squaredSumL += samples[i] * samples[i];
                squaredSumR += samples[i + 1] * samples[i + 1];
            }

            squaredSumL = Math.Sqrt(squaredSumL / sampleCount);
            squaredSumR = Math.Sqrt(squaredSumR / sampleCount);

            double squaredSum = Math.Max(squaredSumL, squaredSumR);

            float scalingFactor = (float)(1.0 / squaredSum);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalingFactor) || float.IsInfinity(scalingFactor))
            {
                scalingFactor = 1f;
                Debug.LogError("NaN Scaling Factor");
            }

            for (int i = 0; i < 2 * sampleCount; i++)
            {
                samples[i] *= scalingFactor;
            }
        }
    }
}

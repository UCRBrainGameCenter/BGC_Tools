using System;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;
using System.Collections.Generic;

namespace BGC.Audio
{
    /// <summary>
    /// A collection of some common procedures related to level scaling of Audio
    /// </summary>
    public static class Normalization
    {
        public enum Scheme
        {
            RMS = 0,
            Peak,
            RMSAssigned,
            MAX
        }

        public const double dbSafetyLimit = 90.0;

        /// <summary>
        /// How far, in dB, the delivered level may exceed the safety limit before a stream is
        /// refused. Absorbs sub-0.1 dB differences between a stream's claimed RMS and its samples
        /// (e.g. a finite sine that isn't a whole number of periods).
        /// </summary>
        public const double deliveredLevelTolerance = 0.1;

        /// <summary>
        /// Throws if <paramref name="stream"/>, scaled so that its claimed RMS
        /// (<paramref name="claimedRMS"/>) plays at <paramref name="requestedLevel"/>, would
        /// actually deliver more than <paramref name="limit"/>.
        /// The delivered level is computed from the stream's own samples: each channel plays at
        /// requestedLevel + 20 log10(sampleRMS / claimedRMS), for any calibration. Infinite streams
        /// can't be measured, so for them only the requested level is checked.
        /// Reads the stream once (and resets it), like <see cref="BGCStreamExtensions.CalculateRMS"/>.
        /// </summary>
        /// <param name="channel">
        /// The input channel whose samples play at <paramref name="requestedLevel"/>, when the
        /// output channels have different requested levels; -1 (the default) checks every channel.
        /// A mono input feeds every output channel, so it is always checked.
        /// </param>
        public static void CheckDeliveredLevel(
            IBGCStream stream,
            double requestedLevel,
            double claimedRMS,
            double limit,
            string unit,
            int channel = -1)
        {
            if (TryMeasureDeliveredLevel(stream, requestedLevel, claimedRMS, unit, channel, out double deliveredLevel))
            {
                SafetyLimit.Resolve(
                    mode: SafetyLimitMode.Refuse,
                    check: "Delivered",
                    unit: unit,
                    limit: limit,
                    tolerance: deliveredLevelTolerance,
                    requestedLevel: requestedLevel,
                    deliveredLevel: deliveredLevel,
                    channel: channel,
                    detail: DeliveredDetail(requestedLevel, deliveredLevel, unit));
            }
        }

        /// <summary>
        /// The level <paramref name="stream"/> would deliver when scaled so that its claimed RMS
        /// plays at <paramref name="requestedLevel"/>: requestedLevel + 20 log10(sampleRMS / claimedRMS),
        /// from the stream's own samples (read once and reset). False when it can't be measured: an
        /// infinite or empty stream, or silence. Throws for a zero or NaN claim, and for samples that
        /// aren't finite (PART-984).
        /// </summary>
        /// <param name="channel">
        /// The input channel that plays at <paramref name="requestedLevel"/>; -1 measures the loudest.
        /// A mono input feeds every output channel, so it is always measured.
        /// </param>
        public static bool TryMeasureDeliveredLevel(
            IBGCStream stream,
            double requestedLevel,
            double claimedRMS,
            string unit,
            int channel,
            out double deliveredLevel)
        {
            deliveredLevel = double.NaN;

            if (stream.ChannelSamples == int.MaxValue || stream.ChannelSamples == 0)
            {
                return false;
            }

            //A zero or NaN claim can't be regulated to any level: refuse it rather than skip the check
            CheckRegulatable(claimedRMS, requestedLevel, unit);

            double[] channelRMS = stream.CalculateRMS().ToArray();

            if (channelRMS.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
            {
                throw new StreamCompositionException(
                    $"Refused a stimulus at {requestedLevel} {unit}: its samples aren't finite numbers (NaN or infinity).");
            }

            double sampleRMS = (channel >= 0 && channel < channelRMS.Length && channelRMS.Length > 1) ?
                channelRMS[channel] :
                channelRMS.DefaultIfEmpty(0.0).Max();

            if (sampleRMS <= 0.0)
            {
                //Silence is below any limit
                return false;
            }

            deliveredLevel = requestedLevel + 20.0 * Math.Log10(sampleRMS / claimedRMS);
            return true;
        }

        /// <summary> The message's account of a delivered-level exceedance </summary>
        public static string DeliveredDetail(double requestedLevel, double deliveredLevel, string unit) =>
            $"Requested Level: {requestedLevel} {unit}, but the stimulus would deliver " +
            $"{deliveredLevel:0.0} {unit} (its samples are {deliveredLevel - requestedLevel:+0.0;-0.0} dB " +
            $"from its claimed RMS)";

        /// <summary>
        /// Applies <paramref name="mode"/> to a requested pair of levels: a request above the
        /// limit is refused, clamped to the limit, or allowed (and reported in every case).
        /// </summary>
        public static (double levelL, double levelR) ResolveRequest(
            SafetyLimitMode mode,
            string unit,
            double levelL,
            double levelR)
        {
            if (levelL == levelR)
            {
                double level = SafetyLimit.Resolve(mode, "Request", unit, dbSafetyLimit, 0.0, levelL, levelL, -1,
                    $"Requested Level: {levelL} {unit}");
                return (level, level);
            }

            return (
                SafetyLimit.Resolve(mode, "Request", unit, dbSafetyLimit, 0.0, levelL, levelL, 0, $"Requested Level: {levelL} {unit}"),
                SafetyLimit.Resolve(mode, "Request", unit, dbSafetyLimit, 0.0, levelR, levelR, 1, $"Requested Level: {levelR} {unit}"));
        }

        /// <summary>
        /// Throws unless <paramref name="rms"/>, the RMS a level regulator scales by, is a positive
        /// finite number. A regulator scales by requestedLevelFactor / rms: at a zero RMS that is
        /// infinite (which the regulators then replaced by 1, playing the stimulus unscaled, usually
        /// silent, while logging the requested level), and a NaN RMS poisons every sample. Either
        /// way the stimulus can't be played at the requested level, whether or not the safety limit
        /// is engaged. A zero RMS means nothing renderable: e.g. every carrier outside 0 Hz to the
        /// Nyquist frequency.
        /// </summary>
        public static void CheckRegulatable(double rms, double requestedLevel, string unit)
        {
            if (!(rms > 0.0) || double.IsInfinity(rms))
            {
                throw new StreamCompositionException(
                    $"Can't play a stimulus at {requestedLevel} {unit}: its RMS is {rms}, so there is nothing " +
                    $"to scale to that level. If it is synthesized, check that its frequencies lie between " +
                    $"0 Hz and the Nyquist frequency ({0.5 * FrequencyDomain.SamplingRate} Hz).");
            }
        }

        public static void GetAmplitudeFactors(
            double dbSPLL,
            double dbSPLR,
            out double factorL,
            out double factorR,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            if (safetyLimit && (dbSPLL > dbSafetyLimit || dbSPLR > dbSafetyLimit))
            {
                throw new StreamCompositionException(
                    $"Tried to exceed safety limit of 90dB without disengaging safety. " +
                    $"Requested Level: {Math.Max(dbSPLL, dbSPLR)} dB");
            }

            (factorL, factorR) = Calibration.GetLevelFactors(
                levelL: dbSPLL,
                levelR: dbSPLR,
                source: source);
        }

        /// <summary>
        /// As above, with a request above the limit refused, clamped to the limit or allowed per
        /// <paramref name="mode"/> (and reported through <see cref="SafetyLimit.EventReporter"/>).
        /// </summary>
        public static void GetAmplitudeFactors(
            double dbSPLL,
            double dbSPLR,
            out double factorL,
            out double factorR,
            Calibration.Source source,
            SafetyLimitMode mode)
        {
            (dbSPLL, dbSPLR) = ResolveRequest(mode, "dB", dbSPLL, dbSPLR);

            (factorL, factorR) = Calibration.GetLevelFactors(
                levelL: dbSPLL,
                levelR: dbSPLR,
                source: source);
        }

        /// <summary>
        /// Normalize the input sound buffer.  Leave destination null to normalize inplace.
        /// </summary>
        /// <param name="samples">Source of samples, destination as well if samples is null</param>
        /// <param name="destination">Leave null to normalize in-place</param>
        public static void Normalize(
            double desiredLevel,
            float[] samples,
            float[] destination = null,
            Scheme scheme = Scheme.RMS,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            if (samples == destination)
            {
                //It is more efficient to leave destination null rather than passing in two
                //references to the same array
                destination = null;
            }

            switch (scheme)
            {
                case Scheme.RMS:
                    NormalizeStereo_RMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        samples: samples,
                        destination: destination);
                    break;

                case Scheme.Peak:
                    NormalizeStereo_Peak(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        samples: samples,
                        destination: destination);
                    break;

                case Scheme.RMSAssigned:
                    Debug.LogError($"You must provide a effectiveRMS to use this Scheme.");
                    goto case Scheme.RMS;

                default:
                    Debug.LogError($"Unexpected Scheme: {scheme}");
                    goto case Scheme.RMS;
            }
        }

        /// <summary>
        /// Normalize the input sound buffer.  Leave destination null to normalize inplace.
        /// </summary>
        /// <param name="samples">Source of samples, destination as well if samples is null</param>
        /// <param name="destination">Leave null to normalize in-place</param>
        public static void Normalize(
            double desiredLevel,
            double effectiveRMS,
            float[] samples,
            float[] destination = null,
            Scheme scheme = Scheme.RMSAssigned,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            if (samples == destination)
            {
                //It is more efficient to leave destination null rather than passing in two
                //references to the same array
                destination = null;
            }

            switch (scheme)
            {
                case Scheme.RMS:
                    Debug.LogError($"Argument effectiveRMS provided, but not meaningful.");
                    NormalizeStereo_RMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        samples: samples,
                        destination: destination);
                    break;

                case Scheme.Peak:
                    Debug.LogError($"Argument effectiveRMS provided, but not meaningful.");
                    NormalizeStereo_Peak(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        samples: samples,
                        destination: destination);
                    break;

                case Scheme.RMSAssigned:
                    NormalizeStereo_TargetRMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        effectiveRMS: effectiveRMS,
                        samples: samples,
                        destination: destination);
                    break;

                default:
                    Debug.LogError($"Unexpected Scheme: {scheme}");
                    goto case Scheme.RMSAssigned;
            }
        }

        /// <summary>
        /// Normalize the input sound buffer.
        /// Leave destination null to allocate a new stereo output array.
        /// </summary>
        public static float[] NormalizeMono(
            double desiredLevel,
            float[] monoInput,
            float[] stereoOutput = null,
            int inputOffset = 0,
            int outputOffset = 0,
            int sampleCount = int.MaxValue,
            Scheme scheme = Scheme.RMS,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            switch (scheme)
            {
                case Scheme.RMS:
                    return NormalizeMono_RMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        monoInput: monoInput,
                        stereoOutput: stereoOutput,
                        inputOffset: inputOffset,
                        outputOffset: outputOffset,
                        sampleCount: sampleCount);

                case Scheme.Peak:
                    return NormalizeMono_Peak(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        monoInput: monoInput,
                        stereoOutput: stereoOutput,
                        inputOffset: inputOffset,
                        outputOffset: outputOffset,
                        sampleCount: sampleCount);

                case Scheme.RMSAssigned:
                    Debug.LogError($"You must provide a effectiveRMS to use this Scheme.");
                    goto case Scheme.RMS;

                default:
                    Debug.LogError($"Unexpected Scheme: {scheme}");
                    goto case Scheme.RMS;
            }
        }

        /// <summary>
        /// Normalize the input sound buffer.
        /// Leave destination null to allocate a new stereo output array.
        /// </summary>
        public static float[] NormalizeMono(
            double desiredLevel,
            double effectiveRMS,
            float[] monoInput,
            float[] stereoOutput = null,
            int inputOffset = 0,
            int outputOffset = 0,
            int sampleCount = int.MaxValue,
            Scheme scheme = Scheme.RMS,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source,
                safetyLimit: safetyLimit);

            switch (scheme)
            {
                case Scheme.RMS:
                    Debug.LogError($"Argument effectiveRMS provided, but not meaningful.");
                    return NormalizeMono_RMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        monoInput: monoInput,
                        stereoOutput: stereoOutput,
                        inputOffset: inputOffset,
                        outputOffset: outputOffset,
                        sampleCount: sampleCount);

                case Scheme.Peak:
                    Debug.LogError($"Argument effectiveRMS provided, but not meaningful.");
                    return NormalizeMono_Peak(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        monoInput: monoInput,
                        stereoOutput: stereoOutput,
                        inputOffset: inputOffset,
                        outputOffset: outputOffset,
                        sampleCount: sampleCount);

                case Scheme.RMSAssigned:
                    Debug.LogError($"You must provide a effectiveRMS to use this Scheme.");
                    return NormalizeMono_TargetRMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        effectiveRMS: effectiveRMS,
                        monoInput: monoInput,
                        stereoOutput: stereoOutput,
                        inputOffset: inputOffset,
                        outputOffset: outputOffset,
                        sampleCount: sampleCount);

                default:
                    Debug.LogError($"Unexpected Scheme: {scheme}");
                    goto case Scheme.RMS;
            }
        }

        #region Stereo Normalizations

        public static void NormalizeStereo_RMS(
            double levelFactorL,
            double levelFactorR,
            float[] samples,
            float[] destination = null)
        {
            bool inplace = (destination == null);

            if (!inplace && destination.Length < samples.Length)
            {
                Debug.LogError($"Destination length ({destination.Length}) shorter than Source length ({samples.Length})");
                return;
            }

            double[] sampleSquaredSum = new double[2];
            int sampleCount = samples.Length / 2;

            for (int i = 0; i < sampleCount; i++)
            {
                sampleSquaredSum[0] += samples[2 * i] * samples[2 * i];
                sampleSquaredSum[1] += samples[2 * i + 1] * samples[2 * i + 1];
            }

            sampleSquaredSum[0] = Math.Sqrt(sampleSquaredSum[0] / sampleCount);
            sampleSquaredSum[1] = Math.Sqrt(sampleSquaredSum[1] / sampleCount);

            double maxRMS = Math.Max(sampleSquaredSum[0], sampleSquaredSum[1]);

            float scalingFactorL = (float)(levelFactorL / maxRMS);
            float scalingFactorR = (float)(levelFactorR / maxRMS);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalingFactorL) || float.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1f;
            }

            if (float.IsNaN(scalingFactorR) || float.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1f;
            }

            if (inplace)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[2 * i] *= scalingFactorL;
                    samples[2 * i + 1] *= scalingFactorR;
                }
            }
            else
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    destination[2 * i] = samples[2 * i] * scalingFactorL;
                    destination[2 * i + 1] = samples[2 * i + 1] * scalingFactorR;
                }
            }
        }

        public static void NormalizeStereo_TargetRMS(
            double levelFactorL,
            double levelFactorR,
            double effectiveRMS,
            float[] samples,
            float[] destination = null)
        {
            bool inplace = (destination == null);

            if (!inplace && destination.Length < samples.Length)
            {
                Debug.LogError($"Destination length ({destination.Length}) shorter than Source length ({samples.Length})");
                return;
            }

            int sampleCount = samples.Length / 2;

            float scalingFactorL = (float)(levelFactorL / effectiveRMS);
            float scalingFactorR = (float)(levelFactorR / effectiveRMS);

            if (inplace)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[2 * i] *= scalingFactorL;
                    samples[2 * i + 1] *= scalingFactorR;
                }
            }
            else
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    destination[2 * i] = samples[2 * i] * scalingFactorL;
                    destination[2 * i + 1] = samples[2 * i + 1] * scalingFactorR;
                }
            }
        }

        /// <summary>
        /// Peak Equivalence Level-Scaling
        /// </summary>
        public static void NormalizeStereo_Peak(
            double levelFactorL,
            double levelFactorR,
            float[] samples,
            float[] destination = null)
        {
            bool inplace = (destination == null);

            if (!inplace && destination.Length < samples.Length)
            {
                Debug.LogError($"Destination length ({destination.Length}) shorter than Source length ({samples.Length})");
                return;
            }

            double maxPeak = 0.0;
            int sampleCount = samples.Length / 2;

            for (int i = 0; i < sampleCount; i++)
            {
                maxPeak = Math.Max(samples[2 * i], maxPeak);
                maxPeak = Math.Max(samples[2 * i + 1], maxPeak);
            }

            float scalingFactorL = (float)(levelFactorL * Calibration.RMS_TO_PEAK / maxPeak);
            float scalingFactorR = (float)(levelFactorR * Calibration.RMS_TO_PEAK / maxPeak);

            if (float.IsNaN(scalingFactorL) || float.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1f;
            }

            if (float.IsNaN(scalingFactorR) || float.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1f;
            }

            if (inplace)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[2 * i] *= scalingFactorL;
                    samples[2 * i + 1] *= scalingFactorR;
                }
            }
            else
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    destination[2 * i] = samples[2 * i] * scalingFactorL;
                    destination[2 * i + 1] = samples[2 * i + 1] * scalingFactorR;
                }
            }
        }

        #endregion Stereo Normalizations
        #region Mono Normalizations

        public static float[] NormalizeMono_RMS(
            double levelFactorL,
            double levelFactorR,
            float[] monoInput,
            float[] stereoOutput = null,
            int inputOffset = 0,
            int outputOffset = 0,
            int sampleCount = int.MaxValue)
        {
            if (sampleCount == int.MaxValue)
            {
                //Set sampleCount if the argument wasn't provided
                if (stereoOutput == null)
                {
                    //Determined by the input if we're creating the output
                    sampleCount = monoInput.Length - inputOffset;
                }
                else
                {
                    //Determined by the smaller of the two if we are not
                    sampleCount = Math.Min(
                        monoInput.Length - inputOffset,
                        (stereoOutput.Length - 2 * outputOffset) / 2);

                }
            }
            else if (monoInput.Length < monoInput.Length - inputOffset)
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Input length ({monoInput.Length}) shorter than required length ({monoInput.Length - inputOffset})");
                return stereoOutput;
            }

            if (stereoOutput == null)
            {
                //Create stereoOutput if the argument wasn't provided
                stereoOutput = new float[2 * (sampleCount + outputOffset)];
            }
            else if (stereoOutput.Length < 2 * (sampleCount + outputOffset))
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Output length ({stereoOutput.Length}) shorter than required length ({2 * (sampleCount + outputOffset)})");
                return stereoOutput;
            }

            double sampleSquaredSum = 0.0;

            for (int i = 0; i < sampleCount; i++)
            {
                sampleSquaredSum += monoInput[i + inputOffset] * monoInput[i + inputOffset];
            }

            double maxRMS = Math.Sqrt(sampleSquaredSum / sampleCount);

            float scalingFactorL = (float)(levelFactorL / maxRMS);
            float scalingFactorR = (float)(levelFactorR / maxRMS);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalingFactorL) || float.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1f;
            }

            if (float.IsNaN(scalingFactorR) || float.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1f;
            }

            for (int i = 0; i < sampleCount; i++)
            {
                stereoOutput[2 * (i + outputOffset)] = monoInput[i + inputOffset] * scalingFactorL;
                stereoOutput[2 * (i + outputOffset) + 1] = monoInput[i + inputOffset] * scalingFactorR;
            }

            return stereoOutput;
        }

        public static float[] NormalizeMono_TargetRMS(
            double levelFactorL,
            double levelFactorR,
            double effectiveRMS,
            float[] monoInput,
            float[] stereoOutput = null,
            int inputOffset = 0,
            int outputOffset = 0,
            int sampleCount = int.MaxValue)
        {
            if (sampleCount == int.MaxValue)
            {
                //Set sampleCount if the argument wasn't provided
                if (stereoOutput == null)
                {
                    //Determined by the input if we're creating the output
                    sampleCount = monoInput.Length - inputOffset;
                }
                else
                {
                    //Determined by the smaller of the two if we are not
                    sampleCount = Math.Min(
                        monoInput.Length - inputOffset,
                        (stereoOutput.Length - 2 * outputOffset) / 2);

                }
            }
            else if (monoInput.Length < monoInput.Length - inputOffset)
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Input length ({monoInput.Length}) shorter than required length ({monoInput.Length - inputOffset})");
                return stereoOutput;
            }

            if (stereoOutput == null)
            {
                //Create stereoOutput if the argument wasn't provided
                stereoOutput = new float[2 * (sampleCount + outputOffset)];
            }
            else if (stereoOutput.Length < 2 * (sampleCount + outputOffset))
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Output length ({stereoOutput.Length}) shorter than required length ({2 * (sampleCount + outputOffset)})");
                return stereoOutput;
            }

            float scalingFactorL = (float)(levelFactorL / effectiveRMS);
            float scalingFactorR = (float)(levelFactorR / effectiveRMS);

            for (int i = 0; i < sampleCount; i++)
            {
                stereoOutput[2 * (i + outputOffset)] = monoInput[i + inputOffset] * scalingFactorL;
                stereoOutput[2 * (i + outputOffset) + 1] = monoInput[i + inputOffset] * scalingFactorR;
            }

            return stereoOutput;
        }

        /// <summary>
        /// Peak Equivalence Level-Scaling
        /// </summary>
        public static float[] NormalizeMono_Peak(
            double levelFactorL,
            double levelFactorR,
            float[] monoInput,
            float[] stereoOutput = null,
            int inputOffset = 0,
            int outputOffset = 0,
            int sampleCount = int.MaxValue)
        {
            if (sampleCount == int.MaxValue)
            {
                //Set sampleCount if the argument wasn't provided
                if (stereoOutput == null)
                {
                    //Determined by the input if we're creating the output
                    sampleCount = monoInput.Length - inputOffset;
                }
                else
                {
                    //Determined by the smaller of the two if we are not
                    sampleCount = Math.Min(
                        monoInput.Length - inputOffset,
                        (stereoOutput.Length - 2 * outputOffset) / 2);

                }
            }
            else if (monoInput.Length < monoInput.Length - inputOffset)
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Input length ({monoInput.Length}) shorter than required length ({monoInput.Length - inputOffset})");
                return stereoOutput;
            }

            if (stereoOutput == null)
            {
                //Create stereoOutput if the argument wasn't provided
                stereoOutput = new float[2 * (sampleCount + outputOffset)];
            }
            else if (stereoOutput.Length < 2 * (sampleCount + outputOffset))
            {
                //Except out if it was provided but was unusable
                Debug.LogError($"Output length ({stereoOutput.Length}) shorter than required length ({2 * (sampleCount + outputOffset)})");
                return stereoOutput;
            }

            double maxPeak = 0.0;
            for (int i = 0; i < sampleCount; i++)
            {
                maxPeak = Math.Max(monoInput[i + inputOffset], maxPeak);
            }

            float scalingFactorL = (float)(levelFactorL * Calibration.RMS_TO_PEAK / maxPeak);
            float scalingFactorR = (float)(levelFactorR * Calibration.RMS_TO_PEAK / maxPeak);

            if (float.IsNaN(scalingFactorL) || float.IsInfinity(scalingFactorL))
            {
                scalingFactorL = 1f;
            }

            if (float.IsNaN(scalingFactorR) || float.IsInfinity(scalingFactorR))
            {
                scalingFactorR = 1f;
            }

            for (int i = 0; i < sampleCount; i++)
            {
                stereoOutput[2 * (i + outputOffset)] = monoInput[i + inputOffset] * scalingFactorL;
                stereoOutput[2 * (i + outputOffset) + 1] = monoInput[i + inputOffset] * scalingFactorR;
            }

            return stereoOutput;
        }

        #endregion Mono Normalizations

        /// <param name="deliveredLevelChannel">
        /// For a stereo stream whose channels have different requested levels: the channel that
        /// plays at <paramref name="desiredLevel"/>, so the safety limit judges that channel only.
        /// -1 (the default) judges every channel.
        /// </param>
        public static void GetRMSScalingFactors(
            IBGCStream stream,
            double desiredLevel,
            out double scalingFactorL,
            out double scalingFactorR,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true,
            int deliveredLevelChannel = -1)
        {
            GetRMSScalingFactors(
                stream: stream,
                desiredLevel: desiredLevel,
                scalingFactorL: out scalingFactorL,
                scalingFactorR: out scalingFactorR,
                source: source,
                mode: SafetyLimit.ModeFor(safetyLimit),
                deliveredLevelChannel: deliveredLevelChannel);
        }

        /// <summary>
        /// The scaling factors that play <paramref name="stream"/> at <paramref name="desiredLevel"/>,
        /// with the safety limit applied per <paramref name="mode"/>: a request above the limit, or
        /// samples that would deliver more than the limit (+0.1 dB) at that request, are refused,
        /// turned down to the limit, or allowed, and reported either way. Under Off the delivered
        /// level is measured only when a reporter is installed.
        /// </summary>
        public static void GetRMSScalingFactors(
            IBGCStream stream,
            double desiredLevel,
            out double scalingFactorL,
            out double scalingFactorR,
            Calibration.Source source,
            SafetyLimitMode mode,
            int deliveredLevelChannel = -1)
        {
            //The request itself: refused, clamped to the limit, or allowed
            double level = SafetyLimit.Resolve(mode, "Request", "dB", dbSafetyLimit, 0.0, desiredLevel, desiredLevel,
                deliveredLevelChannel, $"Requested Level: {desiredLevel} dB");

            GetAmplitudeFactors(
                dbSPLL: level,
                dbSPLR: level,
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

            CheckRegulatable(maxRMS, level, "dB");

            //The claimed RMS may not match the samples, so check the level actually delivered.
            //Samples that would play above the limit are refused, turned down to it, or allowed.
            //Under Off a request above the limit was already reported: one event per stimulus.
            double attenuation = 1.0;
            bool requestReported = mode == SafetyLimitMode.Off && desiredLevel > dbSafetyLimit;

            if (!rmsMeasured && !requestReported && SafetyLimit.WantsMeasurement(mode) &&
                TryMeasureDeliveredLevel(stream, level, maxRMS, "dB", deliveredLevelChannel, out double deliveredLevel))
            {
                double appliedLevel = SafetyLimit.Resolve(
                    mode: mode,
                    check: "Delivered",
                    unit: "dB",
                    limit: dbSafetyLimit,
                    tolerance: deliveredLevelTolerance,
                    requestedLevel: level,
                    deliveredLevel: deliveredLevel,
                    channel: deliveredLevelChannel,
                    detail: DeliveredDetail(level, deliveredLevel, "dB"));

                if (appliedLevel < deliveredLevel)
                {
                    attenuation = Math.Pow(10.0, (appliedLevel - deliveredLevel) / 20.0);
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

        public static void GetRMSScalingFactors(
            float[] stereoSamples,
            double desiredLevel,
            out float scalingFactorL,
            out float scalingFactorR,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
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
            double desiredLevel,
            out double scalingFactorL,
            out double scalingFactorR,
            Calibration.Source source = Calibration.Source.Custom,
            bool safetyLimit = true)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
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

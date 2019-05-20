using System;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;

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
            RMSProscribed,
            MAX
        }

        public const double dbMax = 90.0;
        public const double dbOffset = 80.0;

        public const double RMS_TO_PEAK = 2.8;
        public const double TARGET_RMS = 1.0 / 128.0;
        public const double TARGET_PEAK = TARGET_RMS * RMS_TO_PEAK;

        public static void SPLToAdjustmentDB(
            double dbSPLL,
            double dbSPLR,
            out double dbAdjustL,
            out double dbAdjustR,
            Calibration.Source source = Calibration.Source.Custom)
        {
            dbSPLL = GeneralMath.Clamp(dbSPLL, -60, dbMax);
            dbSPLR = GeneralMath.Clamp(dbSPLR, -60, dbMax);

            //Start with Left calculation
            Calibration.GetLevelOffset(
                level: dbSPLL,
                levelOffsetL: out double dbOffsetL,
                levelOffsetR: out double dbOffsetR,
                source: source);

            dbAdjustL = dbOffsetL + dbSPLL - dbOffset;

            //If they're not the same, then generate a new set of offsets
            if (dbSPLL != dbSPLR)
            {
                //To right calculation if it's different
                Calibration.GetLevelOffset(
                    level: dbSPLR,
                    levelOffsetL: out _,
                    levelOffsetR: out dbOffsetR,
                    source: source);
            }
            dbAdjustR = dbOffsetR + dbSPLR - dbOffset;
        }

        public static void GetAmplitudeFactors(
            double dbSPLL,
            double dbSPLR,
            out double factorL,
            out double factorR,
            Calibration.Source source = Calibration.Source.Custom)
        {
            SPLToAdjustmentDB(
                dbSPLL: dbSPLL,
                dbSPLR: dbSPLR,
                dbAdjustL: out double dbLevelL,
                dbAdjustR: out double dbLevelR,
                source: source);

            factorL = Math.Pow(10.0, dbLevelL / 20.0);
            factorR = Math.Pow(10.0, dbLevelR / 20.0);
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
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

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

                case Scheme.RMSProscribed:
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
            Scheme scheme = Scheme.RMSProscribed,
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

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

                case Scheme.RMSProscribed:
                    NormalizeStereo_TargetRMS(
                        levelFactorL: levelFactorL,
                        levelFactorR: levelFactorR,
                        effectiveRMS: effectiveRMS,
                        samples: samples,
                        destination: destination);
                    break;

                default:
                    Debug.LogError($"Unexpected Scheme: {scheme}");
                    goto case Scheme.RMSProscribed;
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
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

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

                case Scheme.RMSProscribed:
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
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

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

                case Scheme.RMSProscribed:
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

            float scalingFactorL = (float)(levelFactorL * (TARGET_RMS / maxRMS));
            float scalingFactorR = (float)(levelFactorR * (TARGET_RMS / maxRMS));

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

            float scalingFactorL = (float)(levelFactorL * (TARGET_RMS / effectiveRMS));
            float scalingFactorR = (float)(levelFactorR * (TARGET_RMS / effectiveRMS));

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

            float scalingFactorL = (float)(levelFactorL * (TARGET_PEAK / maxPeak));
            float scalingFactorR = (float)(levelFactorR * (TARGET_PEAK / maxPeak));

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

            float scalingFactorL = (float)(levelFactorL * (TARGET_RMS / maxRMS));
            float scalingFactorR = (float)(levelFactorR * (TARGET_RMS / maxRMS));

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

            float scalingFactorL = (float)(levelFactorL * (TARGET_RMS / effectiveRMS));
            float scalingFactorR = (float)(levelFactorR * (TARGET_RMS / effectiveRMS));

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

            float scalingFactorL = (float)(levelFactorL * (TARGET_PEAK / maxPeak));
            float scalingFactorR = (float)(levelFactorR * (TARGET_PEAK / maxPeak));

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

        public static void GetRMSScalingFactors(
            IBGCStream stream,
            double desiredLevel,
            out double scalingFactorL,
            out double scalingFactorR,
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

            double maxRMS = stream.GetChannelRMS().Where(x => !double.IsNaN(x)).Max();

            scalingFactorL = levelFactorL * (TARGET_RMS / maxRMS);
            scalingFactorR = levelFactorR * (TARGET_RMS / maxRMS);

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
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

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

            scalingFactorL = (float)(levelFactorL * (TARGET_RMS / maxRMS));
            scalingFactorR = (float)(levelFactorR * (TARGET_RMS / maxRMS));

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
            Calibration.Source source = Calibration.Source.Custom)
        {
            GetAmplitudeFactors(
                dbSPLL: desiredLevel,
                dbSPLR: desiredLevel,
                factorL: out double levelFactorL,
                factorR: out double levelFactorR,
                source: source);

            double sampleSquaredSum = 0.0;

            for (int i = 0; i < monoSamples.Length; i++)
            {
                sampleSquaredSum += monoSamples[i] * monoSamples[i];
            }

            sampleSquaredSum = Math.Sqrt(sampleSquaredSum / monoSamples.Length);

            scalingFactorL = levelFactorL * (TARGET_RMS / sampleSquaredSum);
            scalingFactorR = levelFactorR * (TARGET_RMS / sampleSquaredSum);

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

        public static double CalculateRMSLevel(float[] samples)
        {
            double[] sampleSquaredSum = new double[2];
            int sampleCount = samples.Length / 2;

            for (int i = 0; i < sampleCount; i++)
            {
                sampleSquaredSum[0] += samples[2 * i] * samples[2 * i];
                sampleSquaredSum[1] += samples[2 * i + 1] * samples[2 * i + 1];
            }

            sampleSquaredSum[0] = Math.Sqrt(sampleSquaredSum[0] / sampleCount);
            sampleSquaredSum[1] = Math.Sqrt(sampleSquaredSum[1] / sampleCount);

            double rmsL = sampleSquaredSum[0];
            double rmsR = sampleSquaredSum[1];

            double levelL = 20.0 * Math.Log10(rmsL / TARGET_RMS) + dbOffset;
            double levelR = 20.0 * Math.Log10(rmsR / TARGET_RMS) + dbOffset;

            if (double.IsNaN(levelL) || double.IsInfinity(levelL))
            {
                levelL = -60.0;
            }

            if (double.IsNaN(levelR) || double.IsInfinity(levelR))
            {
                levelR = -60.0;
            }

            Calibration.GetLevelOffset(
                level: levelL,
                levelOffsetL: out double dbOffsetL,
                levelOffsetR: out double dbOffsetR);

            if (levelL != levelR)
            {
                Calibration.GetLevelOffset(
                    level: levelR,
                    levelOffsetL: out _,
                    levelOffsetR: out dbOffsetR);
            }

            return Math.Max(levelL - dbOffsetL, levelR - dbOffsetR);
        }

        public static void StandardizeSoundRMSMono(float[] samples)
        {
            double squaredSum = 0.0;
            int sampleCount = samples.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                squaredSum += samples[i] * samples[i];
            }

            squaredSum = Math.Sqrt(squaredSum / sampleCount);

            float scalingFactor = (float)(TARGET_RMS / squaredSum);

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

        public static void StandardizeSoundRMSStereo(float[] samples)
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

            float scalingFactor = (float)(TARGET_RMS / squaredSum);

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

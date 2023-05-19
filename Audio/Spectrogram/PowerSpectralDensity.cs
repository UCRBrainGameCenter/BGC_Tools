using System;
using System.Linq;
using BGC.Audio.Envelopes;
using BGC.Extensions;
using BGC.Mathematics;

namespace BGC.Audio.Visualization
{
    public static class PowerSpectralDensity
    {
        public static (double[] psd, double offset) DecomposeAndShift(
            IBGCStream stream,
            int windowOrder = 12,
            int targetChannel = 0)
        {
            float[] samples = GetSamples(stream, targetChannel);
            return DecomposeAndShift(samples, windowOrder);
        }

        public static (double[] psd, double offset) DecomposeAndShift(
            float[] samples,
            int windowOrder = 12)
        {
            double[] spectralValues = Decompose(samples, windowOrder);

            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            for (int i = 0; i < spectralValues.Length; i++)
            {
                if (!double.IsNaN(spectralValues[i]) && !double.IsNegativeInfinity(spectralValues[i]))
                {
                    if (spectralValues[i] > maxValue)
                    {
                        maxValue = spectralValues[i];
                    }

                    if (spectralValues[i] < minValue)
                    {
                        minValue = spectralValues[i];
                    }
                }
            }

            for (int i = 0; i < spectralValues.Length; i++)
            {
                if (double.IsNaN(spectralValues[i]) || double.IsNegativeInfinity(spectralValues[i]))
                {
                    spectralValues[i] = minValue - maxValue;
                }
                else
                {
                    spectralValues[i] -= maxValue;
                }
            }

            return (spectralValues, maxValue);
        }

        public static double[] Decompose(
            IBGCStream stream,
            int windowOrder = 12,
            int targetChannel = 0)
        {
            float[] samples = GetSamples(stream, targetChannel);
            return Decompose(samples, windowOrder);
        }

        private static float[] GetSamples(IBGCStream stream, int targetChannel)
        {
            if (stream.Channels <= targetChannel)
            {
                throw new ArgumentException(
                    $"TargetChannel ({targetChannel}) exceeded stream channels ({stream.Channels})",
                    nameof(targetChannel));
            }
            return stream.IsolateChannel(targetChannel).HardClip().Cache().Samples;
        }

        public static bool IsClipped(float[] samples, float amplitudeThreshold = 0.95f, float proportionThreshold = 0.25f)
        {
            int numClipping = 0;
            foreach (float sample in samples)
            {
                if (Math.Abs(sample) >= amplitudeThreshold)
                {
                    numClipping++;
                }
            }

            return numClipping >= samples.Length * proportionThreshold;
        }

        /// <summary>
        /// Decomposes an input signal into an array of power per frequency using Welch's method
        /// for power spectral density estimation and the Fast Fourier Transform (FFT) for
        /// frequency-domain transformation. The Blackman-Harris window function is used to
        /// minimize spectral leakage.
        /// </summary>
        /// <param name="samples">An array of input signal samples (time-domain).</param>
        /// <param name="windowOrder">The window order, which determines the window size
        /// used for spectral analysis (default is 12). The window size is calculated as
        /// 2^windowOrder.</param>
        /// <param name="overlap">The percentage of overlap between adjacent segments used
        /// in Welch's method (default is 50).</param>
        /// <returns>An array of power values in decibels, representing the power per
        /// frequency for half of the frequency range.</returns>
        public static double[] Decompose(
            float[] samples,
            int windowOrder = 12,
            int overlap = 50)
        {
            int windowSize = (int)Math.Pow(2, windowOrder);
            int halfWindowSize = windowSize / 2;
            int segmentOverlap = windowSize * overlap / 100;
            int segmentStep = windowSize - segmentOverlap;
            int numSegments = Math.Max(1, (samples.Length - segmentOverlap) / segmentStep);

            double[] powerPerFrequency = new double[halfWindowSize];

            // Create Blackman-Harris window
            BlackmanHarrisEnvelope blackmanHarris = new BlackmanHarrisEnvelope(windowSize);

            Complex64[] fftBuffer = new Complex64[windowSize];
            double normalizationFactor = 1.0 / (windowSize * numSegments);

            for (int s = 0; s < numSegments; s++)
            {
                int segmentStart = s * segmentStep;

                // Apply Blackman-Harris window and convert to complex buffer
                for (int i = 0; i < windowSize; i++)
                {
                    int sampleIndex = segmentStart + i;

                    // Use zero-padding for overflowed samples
                    float sampleValue = sampleIndex < samples.Length ? samples[sampleIndex] : 0;

                    float windowValue = blackmanHarris.ReadNextSample();
                    fftBuffer[i] = new Complex64(sampleValue * windowValue, 0);
                }
                blackmanHarris.Reset();

                // Perform the FFT
                Fourier.Forward(fftBuffer);

                // Calculate the power spectrum for this segment and accumulate
                for (int i = 0; i < halfWindowSize; i++)
                {
                    powerPerFrequency[i] += normalizationFactor * fftBuffer[i].MagnitudeSquared;
                }
            }

            // Convert power values to decibels
            for (int i = 0; i < halfWindowSize; i++)
            {
                powerPerFrequency[i] = 10 * Math.Log10(powerPerFrequency[i]);
            }

            return powerPerFrequency;
        }
    }
}

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
            double[] spectralValues = Decompose(stream, windowOrder, targetChannel);

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
            if (stream.Channels <= targetChannel)
            {
                throw new ArgumentException(
                    $"TargetChannel ({targetChannel}) exceeded stream channels ({stream.Channels})",
                    nameof(targetChannel));
            }

            return Decompose(
                samples: stream.IsolateChannel(targetChannel).HardClip().Cache().Samples,
                windowOrder: windowOrder);
        }

        public static double[] Decompose(
            float[] samples,
            int windowOrder = 12)
        {
            //WindowSize is 2 ^ windowOrder
            int windowSize = 1 << windowOrder;

            int sampleOffset = windowSize / 2;

            if (windowOrder == 4)
            {
                throw new ArgumentException("Clip too short to evaluate");
            }

            int windowCount = 1 + (int)Math.Ceiling((samples.Length - windowSize) / (double)sampleOffset);

            if (windowCount < 1)
            {
                windowCount = 1;
            }

            //Our output will be just the real-valued amplitudes
            double[] spectralValues = new double[windowSize / 2];

            Complex64[] fftBuffer = new Complex64[windowSize];

            IBGCEnvelopeStream windowStream = new BlackmanHarrisEnvelope(windowSize);

            // 2 x due to negative frequencies
            // 0.5 x due to overlap
            double amplitudeAdjustant = 1.0 / (windowCount * windowSize);

            for (int window = 0; window < windowCount; window++)
            {
                int specificOffset = sampleOffset * window;
                windowStream.Reset();

                //Copy samples into buffer
                for (int i = 0; i < windowSize; i++)
                {
                    //Set real value
                    if (specificOffset + i >= samples.Length)
                    {
                        fftBuffer[i] = Complex64.Zero;
                    }
                    else
                    {
                        fftBuffer[i] = samples[specificOffset + i] * windowStream.ReadNextSample();
                    }
                }

                Fourier.Forward(fftBuffer);

                for (int i = 0; i < fftBuffer.Length / 2; i++)
                {
                    spectralValues[i] += amplitudeAdjustant * fftBuffer[i].MagnitudeSquared;
                }
            }

            for (int i = 0; i < spectralValues.Length; i++)
            {
                spectralValues[i] = 10.0 * Math.Log10(spectralValues[i]);
            }

            return spectralValues;
        }
    }
}

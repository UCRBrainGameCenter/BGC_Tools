using System;
using BGC.Audio.Envelopes;
using BGC.Extensions;
using BGC.Mathematics;

namespace BGC.Audio.Visualization
{
    public static class PowerSpectralDensity
    {
        public static double[] Decompose(
            IBGCStream stream,
            int windowOrder = 12,
            int targetChannel = 0)
        {
            //WindowSize is 2 ^ windowOrder
            int windowSize = 1 << windowOrder;

            if (stream.Channels <= targetChannel)
            {
                throw new ArgumentException(
                    $"TargetChannel ({targetChannel}) exceeded stream channels ({stream.Channels})",
                    nameof(targetChannel));
            }

            float[] samples = stream.IsolateChannel(targetChannel).Cache().Samples;

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

            IBGCEnvelopeStream hammingWindow = new EnvelopeConcatenator(
                CosineEnvelope.HammingWindow(windowSize / 2, true),
                CosineEnvelope.HammingWindow(windowSize / 2, false));

            double amplitudeAdjustant = 10.0 * Math.Log10(windowSize);

            for (int window = 0; window < windowCount; window++)
            {
                int specificOffset = sampleOffset * window;
                hammingWindow.Reset();

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
                        fftBuffer[i] = samples[specificOffset + i] * hammingWindow.ReadNextSample();
                    }
                }

                Fourier.Forward(fftBuffer);

                for (int i = 0; i < fftBuffer.Length / 2; i++)
                {
                    spectralValues[i] =+ amplitudeAdjustant + 10.0 * Math.Log10(2 * fftBuffer[i].Magnitude);
                }
            }

            for (int i = 0; i < spectralValues.Length; i++)
            {
                spectralValues[i] /= windowCount;
            }

            return spectralValues;
        }
    }
}

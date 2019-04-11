using System;
using BGC.Audio.Envelopes;
using BGC.Extensions;
using BGC.Mathematics;

namespace BGC.Audio.Visualization
{
    public static class Spectrogram
    {
        public static SpectralDecomp Decompose(
            IBGCStream stream,
            int windowCount = 400,
            int windowOrder = 12,
            int targetChannel = 0,
            double minFreq = 0.0,
            double maxFreq = double.PositiveInfinity)
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

            int sampleOffset = (int)((samples.Length - windowSize) / (double)(windowCount - 1));

            //Adjust windowSize to conform to sample size and requirements
            while (sampleOffset <= 0 && windowOrder > 4)
            {
                --windowOrder;
                windowSize = 1 << windowOrder;
                windowCount /= 2;
                sampleOffset = (int)((samples.Length - windowSize) / (double)(windowCount - 1));
            }

            if (windowOrder == 4)
            {
                throw new ArgumentException("Clip too short to evaluate");
            }

            if (maxFreq == double.PositiveInfinity)
            {
                //Default MaxFrequency to be determined by the size of the window
                maxFreq = FrequencyDomain.GetComplexSampleFrequency(windowSize, windowSize / 2);
            }

            //Limit Max Frquency by the size of the window
            maxFreq = Math.Min(maxFreq, FrequencyDomain.GetComplexSampleFrequency(windowSize, windowSize / 2));

            //Limit Min Frequency by the 
            minFreq = Math.Max(minFreq, FrequencyDomain.GetComplexSampleFrequency(windowSize, 1));

            //Our output will be just the real-valued amplitudes
            SpectralDecomp decomp = new SpectralDecomp(minFreq, maxFreq, windowSize, windowCount);

            Complex64[] fftBuffer = new Complex64[windowSize];

            IBGCEnvelopeStream hammingWindow = new EnvelopeConcatenator(
                CosineEnvelope.HammingWindow(windowSize / 2, true),
                CosineEnvelope.HammingWindow(windowSize / 2, false));

            for (int window = 0; window < windowCount; window++)
            {
                int specificOffset = sampleOffset * window;
                hammingWindow.Reset();

                //Copy samples into buffer
                for (int i = 0; i < windowSize; i++)
                {
                    //Set real value
                    fftBuffer[i] = samples[specificOffset + i] * hammingWindow.ReadNextSample();
                }

                Fourier.Forward(fftBuffer);

                decomp.Add(window, fftBuffer);
            }

            return decomp;
        }
    }


    public class SpectralDecomp
    {
        public double MinFreq => freqs[0];
        public double MaxFreq => freqs[freqs.Length - 1];

        public double[,] spectralValues;
        public double[] freqs;

        public double minAmplitude;
        public double maxAmplitude;

        private readonly int fftMinFreqBin;
        private readonly int fftMaxFreqBin;
        private readonly double amplitudeAdjustant;

        public SpectralDecomp(double minFreq, double maxFreq, int windowSize, int windowCount)
        {
            fftMinFreqBin = FrequencyDomain.GetComplexFrequencyBin(windowSize, minFreq);

            //+2 to round outward AND to act as an exclusive upperbound
            fftMaxFreqBin = FrequencyDomain.GetComplexFrequencyBin(windowSize, maxFreq) + 2;
            fftMaxFreqBin = Math.Min(fftMaxFreqBin, windowSize / 2);

            spectralValues = new double[windowCount, fftMaxFreqBin - fftMinFreqBin];

            freqs = new double[fftMaxFreqBin - fftMinFreqBin];

            for (int i = 0; i < freqs.Length; i++)
            {
                freqs[i] = FrequencyDomain.GetComplexSampleFrequency(windowSize, fftMinFreqBin + i);
            }

            amplitudeAdjustant = 10.0 * Math.Log10(windowSize);

            minAmplitude = double.PositiveInfinity;
            maxAmplitude = double.NegativeInfinity;
        }

        public void Add(int window, Complex64[] fftBuffer)
        {
            //Copy the data into the output array
            for (int i = 0; i < freqs.Length; i++)
            {
                spectralValues[window, i] = amplitudeAdjustant + 10.0 * Math.Log10(fftBuffer[fftMinFreqBin + i].Magnitude);

                if (spectralValues[window, i] > maxAmplitude && spectralValues[window, i] != double.PositiveInfinity)
                {
                    maxAmplitude = spectralValues[window, i];
                }

                if (spectralValues[window, i] < minAmplitude && spectralValues[window, i] != double.NegativeInfinity)
                {
                    minAmplitude = spectralValues[window, i];
                }
            }
        }

        public double Linterp(int window, double frequency)
        {
            int lowerBound = freqs.BinarySearchLowerBound(frequency);

            if (lowerBound == -1)
            {
                return spectralValues[window, 0];
            }

            if (lowerBound == freqs.Length - 1)
            {
                return spectralValues[window, lowerBound];
            }

            return GeneralMath.Lerp(
                spectralValues[window, lowerBound],
                spectralValues[window, lowerBound + 1],
                (frequency - freqs[lowerBound]) / (freqs[lowerBound + 1] - freqs[lowerBound]));
        }
    }

}

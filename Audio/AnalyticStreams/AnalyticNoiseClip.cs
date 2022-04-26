using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    public class AnalyticNoiseStream : IAnalyticStream
    {
        public enum AmplitudeDistribution
        {
            Violet = 0,
            Blue,
            White,
            Pink,
            Brown,
            MAX
        }

        int IAnalyticStream.Samples => int.MaxValue;
        public double SamplingRate => 44100.0;

        private readonly Random randomizer;

        private readonly AmplitudeDistribution distribution;
        private readonly double rms;
        private readonly double freqLB;
        private readonly double freqUB;
        private readonly int frequencyCount;

        private Complex64[] fftBuffer = null;

        private int position = 0;
        private int fftBufferSize;

        public AnalyticNoiseStream(
            double rms,
            double freqLB,
            double freqUB,
            int frequencyCount,
            AmplitudeDistribution distribution,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            this.rms = rms;
            this.freqLB = freqLB;
            this.freqUB = freqUB;
            this.frequencyCount = frequencyCount;

            this.distribution = distribution;
        }

        private bool initialized = false;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            //Min of 10 periods of lowest frequency
            int bufferSizeLowerBound = (int)Math.Ceiling(SamplingRate * 10.0 / freqLB);
            fftBufferSize = bufferSizeLowerBound.CeilingToPowerOfTwo();
            fftBuffer = new Complex64[fftBufferSize];

            foreach (ComplexCarrierTone carrierTone in CreateSideBands(freqLB, freqUB, frequencyCount, distribution))
            {
                FrequencyDomain.Populate(
                    buffer: fftBuffer,
                    frequency: carrierTone.frequency,
                    amplitude: carrierTone.amplitude);
            }

            Fourier.Inverse(fftBuffer);

            double currentRMS = 0.0;

            for (int i = 0; i < fftBufferSize; i++)
            {
                currentRMS += fftBuffer[i].Real * fftBuffer[i].Real;
            }

            currentRMS = Math.Sqrt(currentRMS / fftBufferSize);

            double factor = rms / currentRMS;

            for (int i = 0; i < fftBufferSize; i++)
            {
                fftBuffer[i] *= factor;
            }
        }

        public int Read(Complex64[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            //Read...
            int samplesToRead = count;

            while (samplesToRead > 0)
            {
                int readingSamples = Math.Min(samplesToRead, fftBufferSize - position);

                for (int i = 0; i < readingSamples; i++)
                {
                    data[offset + i] = fftBuffer[position + i];
                }

                samplesToRead -= readingSamples;
                offset += readingSamples;
                position += readingSamples;

                if (position == fftBufferSize)
                {
                    position = 0;
                }
            }

            return count;
        }

        public void Reset() => position = 0;

        public void Seek(int position)
        {
            this.position = position % fftBufferSize;
            if (this.position < 0)
            {
                this.position += fftBufferSize;
            }
        }

        public double GetRMS() => rms;
        public PresentationConstraints GetPresentationConstraints() => null;

        private IEnumerable<ComplexCarrierTone> CreateSideBands(
            double freqLB,
            double freqUB,
            int count,
            AmplitudeDistribution distribution)
        {
            double freqRatio = Math.Pow((freqUB / freqLB), 1.0 / (count - 1));
            if (double.IsNaN(freqRatio) || double.IsInfinity(freqRatio))
            {
                freqRatio = 1.0;
            }

            double freq = freqLB;

            for (int carrierTone = 0; carrierTone < count; carrierTone++)
            {
                yield return new ComplexCarrierTone(
                    frequency: freq,
                    amplitude: Complex64.FromPolarCoordinates(
                        magnitude: GetFactor(distribution, freq) * CustomRandom.RayleighDistribution(randomizer.NextDouble()),
                        phase: 2.0 * Math.PI * randomizer.NextDouble()));

                freq *= freqRatio;
            }
        }

        //This function includes an extra factors of sqrt(f) to account for the inherent 1/sqrt(f) from
        //the exponential distribution of frequencies
        private double GetFactor(AmplitudeDistribution amplitudeDistribution, double frequency)
        {
            switch (amplitudeDistribution)
            {
                case AmplitudeDistribution.Violet: return frequency * Math.Sqrt(frequency);
                case AmplitudeDistribution.Blue: return frequency;
                case AmplitudeDistribution.White: return Math.Sqrt(frequency);
                case AmplitudeDistribution.Pink: return 1.0;
                case AmplitudeDistribution.Brown: return 1.0 / Math.Sqrt(frequency);

                case AmplitudeDistribution.MAX:
                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeFactor: {amplitudeDistribution}");
                    return 1.0;
            }
        }

        public void Dispose()
        {
        }
    }
}

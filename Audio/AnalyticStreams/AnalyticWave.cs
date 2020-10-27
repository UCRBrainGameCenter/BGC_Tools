using System;
using System.Collections.Generic;
using BGC.Mathematics;

using static System.Math;

namespace BGC.Audio.AnalyticStreams
{
    public class AnalyticWave : IAnalyticStream
    {
        public double SamplingRate => 44100.0;

        int IAnalyticStream.Samples => int.MaxValue;

        private readonly double frequency;
        private readonly double amplitude;
        private readonly double phase;

        private Complex64 partial;
        private readonly double cyclePartial;
        private readonly Complex64[] samples;
        private int position = 0;
        private int cycles = 0;

        public AnalyticWave(double amplitude, double frequency, double phase = 0.0)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.phase = phase;

            double sampleCount = SamplingRate / this.frequency;
            int intSampleCount = (int)Ceiling(sampleCount) - 1;

            cyclePartial = (2 * PI * this.frequency / SamplingRate ) * (intSampleCount - sampleCount);

            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);

            samples = new Complex64[intSampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Complex64.FromPolarCoordinates(
                    magnitude: this.amplitude,
                    phase: this.phase + 2 * PI * i / sampleCount);
            }
        }

        void IAnalyticStream.Initialize() { }

        public int Read(Complex64[] data, int offset, int count)
        {
            int samplesToRead = count;

            while (samplesToRead > 0)
            {
                int readingSamples = Min(samplesToRead, samples.Length - position);

                for (int i = 0; i < readingSamples; i++)
                {
                    data[offset + i] = samples[position + i] * partial;
                }

                samplesToRead -= readingSamples;
                offset += readingSamples;
                position += readingSamples;

                if (position == samples.Length)
                {
                    position = 0;
                    cycles++;
                    partial = Complex64.FromPolarCoordinates(
                        magnitude: 1.0,
                        phase: cycles * cyclePartial);
                }
            }

            return count;
        }

        public void Reset()
        {
            position = 0;
            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);
        }

        public void Seek(int position)
        {
            if (position >= 0)
            {
                cycles = position / samples.Length;
            }
            else
            {
                cycles = (position - samples.Length + 1) / samples.Length;
            }

            this.position = position - cycles * samples.Length;

            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);
        }

        public double GetRMS() => amplitude * Sqrt(0.5);

        public void Dispose()
        {
        }
    }
}

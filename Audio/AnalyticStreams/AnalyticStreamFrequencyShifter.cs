using System;
using System.Collections.Generic;
using BGC.Mathematics;

using static System.Math;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// Applies FrequencyShift to the underlying stream.
    /// Utilizes the FFT Shift Theorem
    /// </summary>
    public class AnalyticStreamFrequencyShifter : SimpleAnalyticFilter
    {
        public override int Samples => stream.Samples;

        private readonly double frequencyShift;

        private Complex64 partial;
        private readonly double cyclePartial;
        private readonly Complex64[] shifterSamples;
        private int position = 0;
        private int cycles = 0;

        public AnalyticStreamFrequencyShifter(
            IAnalyticStream stream,
            double frequencyShift)
            : base(stream)
        {
            this.frequencyShift = frequencyShift;

            double sampleCount = SamplingRate / this.frequencyShift;
            int intSampleCount = (int)Ceiling(sampleCount) - 1;

            cyclePartial = (2 * PI * this.frequencyShift / SamplingRate) * (intSampleCount - sampleCount);

            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);

            shifterSamples = new Complex64[intSampleCount];

            for (int i = 0; i < shifterSamples.Length; i++)
            {
                shifterSamples[i] = Complex64.FromPolarCoordinates(
                    magnitude: 1.0,
                    phase: 2 * PI * i / sampleCount);
            }
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            int samplesRead = stream.Read(data, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                data[offset + i] *= shifterSamples[position++] * partial;
                if (position == shifterSamples.Length)
                {
                    position = 0;
                    cycles++;
                    partial = Complex64.FromPolarCoordinates(
                        magnitude: 1.0,
                        phase: cycles * cyclePartial);
                }
            }

            return samplesRead;
        }

        public override void Reset()
        {
            stream.Reset();
            position = 0;
            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, Samples);
            stream.Seek(position);
            cycles = position / shifterSamples.Length;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);
            this.position = position % shifterSamples.Length;
        }

        public override double GetRMS() => stream.GetRMS();
    }
}

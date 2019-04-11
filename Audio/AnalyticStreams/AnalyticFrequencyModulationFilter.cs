using System;
using System.Collections.Generic;
using BGC.Mathematics;

using static System.Math;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// Applies FrequencyModulation to the underlying stream, in the frequency domain.
    /// Utilizes the FFT Shift Theorem
    /// </summary>
    public class AnalyticFrequencyModulationFilter : SimpleAnalyticFilter
    {
        public override int Samples => stream.Samples;

        private readonly Complex64[] modulator;
        private readonly int modulatorPeriodSamples;

        private int modulatorPosition = 0;

        public AnalyticFrequencyModulationFilter(
            IAnalyticStream stream,
            double modRate,
            double modDepth)
            : base(stream)
        {
            modulatorPeriodSamples = (int)Abs(Round(SamplingRate / modRate));
            modRate = Sign(modRate) * SamplingRate / modulatorPeriodSamples;

            modulator = new Complex64[modulatorPeriodSamples];

            for (int i = 0; i < modulatorPeriodSamples; i++)
            {
                modulator[i] = Complex64.FromPolarCoordinates(
                    magnitude: 1.0,
                    phase: (modDepth / modRate) * Sin(2.0 * PI * i / modulatorPeriodSamples));
            }
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            int samplesRead = stream.Read(data, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                data[offset + i] *= modulator[modulatorPosition++];
                if (modulatorPosition == modulatorPeriodSamples)
                {
                    modulatorPosition = 0;
                }
            }

            return samplesRead;
        }

        public override void Reset()
        {
            modulatorPosition = 0;
            stream.Reset();
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, Samples);
            stream.Seek(position);
            modulatorPosition = position % modulatorPeriodSamples;
        }

        public override double GetRMS() => stream.GetRMS();
    }
}

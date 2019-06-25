using System;
using System.Collections.Generic;
using BGC.Mathematics;
using BGC.Audio.Envelopes;

using static System.Math;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Sine wave stream with specified amplitude, frequency, and initialPhase.
    /// One period is cached and the phase offset is tracked and applied as a
    /// complex number.
    /// </summary>
    public class SineWave : BGCStream, IBGCEnvelopeStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly double amplitude;

        private Complex64 partial;
        private readonly double cyclePartial;
        private readonly Complex64[] samples;
        private int position = 0;
        private int cycles = 0;

        public SineWave(double amplitude, double frequency, double initialPhase = 0.0)
        {
            if (frequency == 0.0)
            {
                throw new ArgumentException($"Unable to render 0Hz Sine Wave");
            }

            this.amplitude = amplitude;

            double sampleCount = SamplingRate / frequency;
            int intSampleCount = (int)Ceiling(sampleCount) - 1;

            cyclePartial = (2 * PI * frequency / SamplingRate) * (intSampleCount - sampleCount);

            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);

            samples = new Complex64[intSampleCount];

            //Initial implementation was actually Cosine.  Duh...
            //Need to subtract Pi/2 to make it the advertised sine wave.
            initialPhase -= 0.5 * PI;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Complex64.FromPolarCoordinates(
                    magnitude: amplitude,
                    phase: initialPhase + 2 * PI * i / sampleCount);
            }
        }

        public SineWave(Complex64 amplitude, double frequency)
        {
            if (frequency == 0.0)
            {
                throw new ArgumentException($"Unable to render 0Hz Sine Wave");
            }

            this.amplitude = amplitude.Magnitude;

            double sampleCount = SamplingRate / frequency;
            int intSampleCount = (int)Ceiling(sampleCount) - 1;

            cyclePartial = (2 * PI * frequency / SamplingRate) * (intSampleCount - sampleCount);

            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);

            samples = new Complex64[intSampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = amplitude.Rotation(2 * PI * i / sampleCount);
            }
        }

        public SineWave(ComplexCarrierTone carrier)
            : this(carrier.amplitude, carrier.frequency)
        {
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToRead = count;

            while (samplesToRead > 0)
            {
                int readingSamples = Min(samplesToRead, samples.Length - position);

                for (int i = 0; i < readingSamples; i++)
                {
                    data[offset + i] = (float)samples[position + i].RealProduct(partial);
                }

                samplesToRead -= readingSamples;
                offset += readingSamples;
                position += readingSamples;

                if (position == samples.Length)
                {
                    //Reset position and advance cycle
                    position = 0;
                    cycles++;
                    partial = Complex64.FromPolarCoordinates(
                        magnitude: 1.0,
                        phase: cycles * cyclePartial);
                }
            }

            return count;
        }

        public override void Reset()
        {
            position = 0;
            cycles = 0;
            partial = Complex64.FromPolarCoordinates(
                magnitude: 1.0,
                phase: cycles * cyclePartial);
        }

        public override void Seek(int position)
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

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() =>
            _channelRMS ?? (_channelRMS = new double[] { amplitude * Sqrt(0.5) });

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => int.MaxValue;
        bool IBGCEnvelopeStream.HasMoreSamples() => true;
        float IBGCEnvelopeStream.ReadNextSample()
        {
            float value = (float)samples[position++].RealProduct(partial);

            if (position == samples.Length)
            {
                //Reset position and advance cycle
                position = 0;
                cycles++;
                partial = Complex64.FromPolarCoordinates(
                    magnitude: 1.0,
                    phase: cycles * cyclePartial);
            }

            return value;
        }

        #endregion IBGCEnvelopeStream
    }
}

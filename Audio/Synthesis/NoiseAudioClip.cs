using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Limited duration noise stream
    /// </summary>
    public class NoiseAudioClip : BGCAudioClip
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

        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        private int _channelSamples;
        public override int ChannelSamples => _channelSamples;

        private readonly Random randomizer;

        private readonly AmplitudeDistribution distribution;
        private readonly double rms;
        private readonly double freqLB;
        private readonly double freqUB;
        private readonly int frequencyCount;

        private Complex64[] fftBuffer = null;

        private int position = 0;

        public NoiseAudioClip(
            double duration,
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

            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
        }

        protected override void _Initialize()
        {
            int fftBufferSize = _channelSamples.CeilingToPowerOfTwo();
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

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            //Read...

            int samplesToRead = Math.Min(count, _channelSamples - position);

            for (int i = 0; i < samplesToRead; i++)
            {
                data[offset + i] = (float)fftBuffer[position + i].Real;
            }

            position += samplesToRead;

            return samplesToRead;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) => 
            this.position = GeneralMath.Clamp(position, 0, _channelSamples);

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return rms;
        }

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

                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeFactor: {amplitudeDistribution}");
                    return 1.0;
            }
        }
    }
}

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
        private float[] samples = null;

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
            //Read clear of the frame's wrap-around, as STMAudioClip and the FD(Single) composer: the
            //first and last 20 ms of a token were -0.5 dB (the truncated sideband series' transient)
            //until 2026-10. A frame with at least 1/8 unused, the output from the middle of the
            //unused part, each carrier time-shifted to keep its phase at the first output sample,
            //Hann-tapered synthesis divided out.
            int fftBufferSize = SingleFrequencyDomainToneComposer.FrameSize(_channelSamples);
            int outputOffset = (fftBufferSize - _channelSamples) / 2;
            double timeShift = -outputOffset / (double)SamplingRate;
            bool taper = FrequencyDomain.HannTaper(fftBufferSize, outputOffset) >= SingleFrequencyDomainToneComposer.MinimumTaperAtOutput;
            fftBuffer = new Complex64[fftBufferSize];

            foreach (ComplexCarrierTone carrierTone in CreateSideBands(freqLB, freqUB, frequencyCount, distribution))
            {
                ComplexCarrierTone shifted = carrierTone.TimeShift(timeShift);
                if (taper)
                {
                    FrequencyDomain.PopulateHannTapered(fftBuffer, shifted);
                }
                else
                {
                    FrequencyDomain.Populate(
                        buffer: fftBuffer,
                        frequency: shifted.frequency,
                        amplitude: shifted.amplitude);
                }
            }

            Fourier.Inverse(fftBuffer);

            samples = new float[_channelSamples];
            for (int i = 0; i < _channelSamples; i++)
            {
                double sample = fftBuffer[outputOffset + i].Real;
                if (taper)
                {
                    sample /= FrequencyDomain.HannTaper(fftBufferSize, outputOffset + i);
                }
                samples[i] = (float)sample;
            }

            fftBuffer = null;

            //The level of the samples the clip returns, which is what it claims
            double currentRMS = 0.0;

            for (int i = 0; i < _channelSamples; i++)
            {
                currentRMS += (double)samples[i] * samples[i];
            }

            currentRMS = Math.Sqrt(currentRMS / _channelSamples);

            if (!(currentRMS > 0.0))
            {
                //rms / 0 would scale every (zero) sample to NaN
                throw new StreamCompositionException(
                    $"Noise from {freqLB} to {freqUB} Hz has no carrier in the renderable range " +
                    $"(0 to {0.5 * SamplingRate} Hz), so it can't be played at its level.");
            }

            float factor = (float)(rms / currentRMS);

            for (int i = 0; i < _channelSamples; i++)
            {
                samples[i] *= factor;
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

            Array.Copy(samples, position, data, offset, samplesToRead);

            position += samplesToRead;

            return samplesToRead;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) => 
            this.position = GeneralMath.Clamp(position, 0, _channelSamples);

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() =>
            channelRMS ?? (channelRMS = new double[1] { rms });

        private readonly IEnumerable<PresentationConstraints> presentationConstraints = new PresentationConstraints[1] { null };
        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;

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

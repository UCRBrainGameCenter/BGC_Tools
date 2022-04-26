using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Spectrotemporally modulated noise with the specified duration.
    /// AKA Ripples.
    /// </summary>
    public class STMAudioClip : BGCAudioClip
    {
        public enum RippleDirection
        {
            Up = 0,
            Down,
            MAX
        }

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

        private readonly Bessel.DoubleBesselCache besselCache = new Bessel.DoubleBesselCache();
        private readonly Dictionary<int, double> sideBandAmplitudes = new Dictionary<int, double>();

        private readonly double modulationDepth;
        private readonly double spectralModulationRate;
        private readonly double temporalModulationRate;

        private readonly IEnumerable<ComplexCarrierTone> carrierToneGenerator;

        public delegate IEnumerable<ComplexCarrierTone> SideBandGenerator(double modulationDepth, ComplexCarrierTone carrierTone, Dictionary<int, double> sidebandAmplitudeCache, double modulationRate, double spectralPhase);
        private readonly SideBandGenerator sideBandGenerator;

        private Complex64[] fftBuffer = null;

        private int position = 0;

        public STMAudioClip(
            double duration,
            double freqLB,
            double freqUB,
            int frequencyCount,
            double modulationDepth,
            double spectralModulationRate,
            double temporalModulationRate,
            RippleDirection rippleDirection,
            AmplitudeDistribution distribution,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            carrierToneGenerator = CreateSideBands(freqLB, freqUB, frequencyCount, distribution);
            sideBandGenerator = ExpCarrierToneSideBands;

            this.modulationDepth = modulationDepth;
            this.temporalModulationRate = temporalModulationRate;

            switch (rippleDirection)
            {
                case RippleDirection.Up:
                    this.spectralModulationRate = spectralModulationRate;
                    break;

                case RippleDirection.Down:
                    this.spectralModulationRate = -spectralModulationRate;
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected RippleDirection: {rippleDirection}");
                    break;
            }

            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
        }

        public STMAudioClip(
            double duration,
            IEnumerable<ComplexCarrierTone> carrierTones,
            double modulationDepth,
            double spectralModulationRate,
            double temporalModulationRate,
            SideBandGenerator sideBandGenerator,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            carrierToneGenerator = carrierTones;
            this.sideBandGenerator = sideBandGenerator;

            this.modulationDepth = modulationDepth;
            this.temporalModulationRate = temporalModulationRate;
            this.spectralModulationRate = spectralModulationRate;

            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
        }

        protected override void _Initialize()
        {
            int fftBufferSize = _channelSamples.CeilingToPowerOfTwo();

            fftBuffer = new Complex64[fftBufferSize];

            sideBandAmplitudes.Clear();

            double spectralModulatorTerm = -2.0 * Math.PI * spectralModulationRate / Math.Log(2.0);
            double spectralStartingPhase = 2.0 * Math.PI * randomizer.NextDouble();

            foreach (ComplexCarrierTone carrierBaseTone in carrierToneGenerator)
            {
                foreach (ComplexCarrierTone sideBand in sideBandGenerator(
                    modulationDepth: modulationDepth,
                    carrierTone: carrierBaseTone,
                    sidebandAmplitudeCache: sideBandAmplitudes,
                    modulationRate: temporalModulationRate,
                    spectralPhase: spectralStartingPhase + spectralModulatorTerm * Math.Log(carrierBaseTone.frequency)))
                {
                    FrequencyDomain.Populate(
                        buffer: fftBuffer,
                        frequency: sideBand.frequency,
                        amplitude: sideBand.amplitude);
                }
            }

            Fourier.Inverse(fftBuffer);
        }


        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

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

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() =>
            channelRMS ?? (channelRMS = this.CalculateRMS());

        private readonly IEnumerable<PresentationConstraints> presentationConstraints = new PresentationConstraints[1] { null };
        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;

        private IEnumerable<ComplexCarrierTone> CreateSideBands(
            double freqLB,
            double freqUB,
            int count,
            AmplitudeDistribution distribution)
        {
            double freqRatio = Math.Pow((freqUB / freqLB), 1.0 / (count - 1.0));
            if (double.IsNaN(freqRatio) || double.IsInfinity(freqRatio))
            {
                freqRatio = 1.0;
            }

            double freq = freqLB;

            for (int carrierTone = 0; carrierTone < count; carrierTone++)
            {
                yield return new ComplexCarrierTone(frequency: freq,
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

        private IEnumerable<ComplexCarrierTone> ExpCarrierToneSideBands(
            double depth,
            ComplexCarrierTone carrierTone,
            Dictionary<int, double> sideBandAmplitudeCache,
            double modulationRate,
            double spectralPhase)
        {
            const int bandBounds = 5;

            for (int band = -1 * bandBounds; band <= bandBounds; band++)
            {
                yield return new ComplexCarrierTone(
                    frequency: carrierTone.frequency + band * modulationRate,
                    amplitude: carrierTone.amplitude * Complex64.FromPolarCoordinates(
                        magnitude: GetExpBandAmplitude(depth, sideBandAmplitudeCache, band),
                        phase: band * spectralPhase + GetBandPhase(band)));
            }
        }

        private static double GetBandPhase(int band)
        {
            if (Math.Abs(band) % 2 == 1)
            {
                //Add -pi/2 phase shift to odd bands
                return -0.5 * Math.PI;
            }

            return 0.0;
        }

        private double GetExpBandAmplitude(
            double depth,
            Dictionary<int, double> sideBandAmplitudeCache,
            int band)
        {
            if (sideBandAmplitudeCache.ContainsKey(band))
            {
                return sideBandAmplitudeCache[band];
            }

            double amplitude;

            double argument = depth * Math.Log(10.0) / 20.0;

            if (band == 0)
            {
                amplitude = Bessel.Bessi(
                    argument: argument,
                    order: 0,
                    cache: besselCache);
            }
            else if (Math.Abs(band) % 2 == 0)
            {
                //Even
                double sign = (Math.Abs(band) / 2) % 2 == 1 ? -1.0 : 1.0;
                amplitude = sign * Bessel.Bessi(
                    argument: argument,
                    order: Math.Abs(band),
                    cache: besselCache);
            }
            else
            {
                //Odd
                double sign = ((Math.Abs(band) - 1) / 2) % 2 == 1 ? -1.0 : 1.0;

                if (band > 0)
                {
                    sign *= -1.0;
                }

                amplitude = sign * Bessel.Bessi(
                    argument: argument,
                    order: Math.Abs(band),
                    cache: besselCache);
            }

            sideBandAmplitudeCache[band] = amplitude;

            return amplitude;
        }
    }
}

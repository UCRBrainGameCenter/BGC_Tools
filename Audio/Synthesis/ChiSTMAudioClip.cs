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
    public class ChiSTMAudioClip : BGCAudioClip
    {
        public enum RippleDirection
        {
            Up = 0,
            Down,
            MAX
        }

        public enum ComponentSpacing
        {
            Harmonic = 0,
            Log,
            MAX
        }

        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        private int _channelSamples;
        public override int ChannelSamples => _channelSamples;

        private readonly Random randomizer;

        private readonly double lowestFrequency;
        private readonly double componentBandwidth;
        private readonly double frequencySpacing;

        private readonly double modulationDepth;
        private readonly double spectralModulationRate;
        private readonly double temporalModulationRate;

        private readonly double maxTemporalVelocity;

        private readonly ComponentSpacing componentSpacing;

        private Complex64[] fftBuffer = null;

        private int position = 0;

        public ChiSTMAudioClip(
            double duration,
            double lowestFrequency,
            double componentBandwidth,
            double frequencySpacing,
            double modulationDepth,
            double spectralModulationRate,
            double temporalModulationRate,
            double maxTemporalVelocity,
            RippleDirection rippleDirection,
            ComponentSpacing componentSpacing,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            this.lowestFrequency = lowestFrequency;
            this.componentBandwidth = componentBandwidth;
            this.frequencySpacing = frequencySpacing;
            this.componentSpacing = componentSpacing;

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

            this.maxTemporalVelocity = maxTemporalVelocity;


            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
        }

        protected override void _Initialize()
        {
            int fftBufferSize = _channelSamples.CeilingToPowerOfTwo();

            fftBuffer = new Complex64[fftBufferSize];

            double effectiveDuration = fftBufferSize / SamplingRate;

            double t_step = effectiveDuration / (2.0 * Math.Round(maxTemporalVelocity * effectiveDuration));
            int t_env_size = 2 * (int)Math.Round(effectiveDuration / (2.0 * t_step));

            double[] carrierFrequencies;

            switch (componentSpacing)
            {
                case ComponentSpacing.Harmonic:
                    {
                        int componentLB = (int)Math.Round(lowestFrequency / frequencySpacing);
                        int componentUB = (int)Math.Round(lowestFrequency * Math.Pow(2, componentBandwidth) / frequencySpacing) + 1;

                        int componentCount = componentUB - componentLB;
                        carrierFrequencies = new double[componentCount];

                        for (int n = 0; n < componentCount; n++)
                        {
                            carrierFrequencies[n] = frequencySpacing * (n + componentLB);
                        }
                    }
                    break;

                case ComponentSpacing.Log:
                    {
                        int componentUB = (int)(Math.Round(2 * componentBandwidth / frequencySpacing) / 2);

                        carrierFrequencies = new double[componentUB];

                        for (int n = 0; n < componentUB; n++)
                        {
                            carrierFrequencies[n] = lowestFrequency * Math.Pow(2.0, n * frequencySpacing);
                        }
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected ComponentSpacing: {componentSpacing}");
                    goto case ComponentSpacing.Log;
            }

            int f_env_size = carrierFrequencies.Length;

            double[] t_env_phases_sin = new double[t_env_size];
            double[] t_env_phases_cos = new double[t_env_size];

            double t_env_phase;
            double t_env_phase_factor = 2.0 * Math.PI * temporalModulationRate;
            for (int t = 0; t < t_env_size; t++)
            {
                t_env_phase = t * t_env_phase_factor * t_step;
                t_env_phases_sin[t] = Math.Sin(t_env_phase);
                t_env_phases_cos[t] = Math.Cos(t_env_phase);
            }

            double value;
            double f_env_phase;
            double f_env_phase_factor = 2.0 * Math.PI * spectralModulationRate;
            double f_env_phase_offset = 0.5 * Math.PI;
            double f_env_phase_sin;
            double f_env_phase_cos;
            Complex64[] complexProfile = new Complex64[t_env_size];
            for (int c = 0; c < f_env_size; c++)
            {
                int f_index = (int)Math.Round(carrierFrequencies[c] * effectiveDuration);
                f_env_phase = Math.Log(carrierFrequencies[c] / lowestFrequency, 2.0) * f_env_phase_factor + f_env_phase_offset;
                f_env_phase_sin = Math.Sin(f_env_phase);
                f_env_phase_cos = Math.Cos(f_env_phase);

                for (int t = 0; t < t_env_size; t++)
                {
                    value = f_env_phase_sin * t_env_phases_cos[t] + f_env_phase_cos * t_env_phases_sin[t];
                    complexProfile[t] = Math.Pow(10.0, modulationDepth * value / 20.0);
                }

                Fourier.Forward(complexProfile);
                FFTShift(complexProfile);

                double componentPhase = 2.0 * Math.PI * randomizer.NextDouble();
                for (int t = 0; t < t_env_size; t++)
                {
                    complexProfile[t] *= Complex64.FromPolarCoordinates(
                        magnitude: fftBufferSize / (2.0 * t_env_size),
                        phase: componentPhase);
                }

                int leftPad = f_index - (t_env_size / 2) - 1;
                int rightPad = (fftBufferSize / 2) - f_index - (t_env_size / 2);

                if (leftPad >= 0 && rightPad >= 0)
                {
                    for (int i = 0; i < t_env_size; i++)
                    {
                        int index = i + leftPad + 1;
                        fftBuffer[index] += complexProfile[i];
                        fftBuffer[fftBufferSize - index] += complexProfile[i].Conjugate();
                    }
                }
                else if (leftPad < 0 && rightPad > 0)
                {
                    for (int i = -leftPad; i < t_env_size; i++)
                    {
                        int index = i + leftPad + 1;
                        fftBuffer[index] += complexProfile[i];
                        fftBuffer[fftBufferSize - index] += complexProfile[i].Conjugate();
                    }
                }
                else if (leftPad > 0 && rightPad < 0)
                {
                    for (int i = 0; i < t_env_size + rightPad; i++)
                    {
                        int index = i + leftPad + 1;
                        fftBuffer[index] += complexProfile[i];
                        fftBuffer[fftBufferSize - index] += complexProfile[i].Conjugate();
                    }
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

        public override IEnumerable<double> GetChannelRMS() => this.CalculateRMS();

        private static void FFTShift(Complex64[] samples)
        {
            Complex64 temp;
            int midPoint = (samples.Length + 1) / 2;
            int endPoint = samples.Length / 2;
            for (int i = 0; i < endPoint; i++)
            {
                temp = samples[i];
                samples[i] = samples[midPoint + i];
                samples[midPoint + i] = temp;
            }
        }
    }
}

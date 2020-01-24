using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// A single-frame rendering of a set of carrier tones. 
    /// </summary>
    public class SingleFrequencyDomainToneComposer : BGCStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => Samples.Length;
        public override int ChannelSamples => Samples.Length;

        private readonly ComplexCarrierTone[] carrierTones;

        public float[] Samples { get; }
        public int Position { get; private set; } = 0;

        private readonly TransformRMSBehavior rmsBehavior;

        public SingleFrequencyDomainToneComposer(
            IEnumerable<ComplexCarrierTone> carrierTones,
            int sampleCount,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
        {
            this.carrierTones = carrierTones.ToArray();
            Samples = new float[sampleCount];

            this.rmsBehavior = rmsBehavior;
        }

        public SingleFrequencyDomainToneComposer(
            IEnumerable<ComplexCarrierTone> carrierTones,
            double duration,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
        {
            this.carrierTones = carrierTones.ToArray();
            int sampleCount = (int)Math.Ceiling(duration * SamplingRate);
            Samples = new float[sampleCount];

            this.rmsBehavior = rmsBehavior;
        }

        protected override void _Initialize()
        {
            int frameSize = Samples.Length.CeilingToPowerOfTwo();
            double outputScalar = 2.0 / Math.Sqrt(frameSize);

            Complex64[] ifftBuffer = new Complex64[frameSize];

            foreach (ComplexCarrierTone carrierTone in carrierTones)
            {
                FrequencyDomain.Populate(ifftBuffer, carrierTone);
            }

            Fourier.Inverse(ifftBuffer);

            for (int i = 0; i < Samples.Length; i++)
            {
                Samples[i] = (float)(outputScalar * ifftBuffer[i].Real);
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesToCopy = Math.Min(count, ChannelSamples - Position);

            Array.Copy(
                sourceArray: Samples,
                sourceIndex: Position,
                destinationArray: data,
                destinationIndex: offset,
                length: samplesToCopy);

            Position += samplesToCopy;

            return samplesToCopy;
        }

        public override void Reset()
        {
            Position = 0;
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        if (!initialized)
                        {
                            Initialize();
                        }
                        _channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        double rms = carrierTones.Select(x => 0.5 * x.amplitude.MagnitudeSquared).Sum();
                        _channelRMS = new double[] { Math.Sqrt(rms) };
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return _channelRMS;
        }
    }
}

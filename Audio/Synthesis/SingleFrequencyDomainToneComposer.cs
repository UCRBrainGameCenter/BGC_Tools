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
            //Populate writes A*sqrt(N) into a single (positive-frequency) bin and the inverse FFT
            //is unscaled, so Re(ifft) is A*sqrt(N)*cos(...). Scaling by 1/sqrt(N) makes each
            //carrier's amplitude A its peak amplitude, as in SineWave, which is what the
            //Passthrough RMS below assumes. (It was 2/sqrt(N), copied from the Continuous
            //composer, where the 2 compensates its Hamming overlap-add; that played +6.02 dB hot.)
            double outputScalar = 1.0 / Math.Sqrt(frameSize);

            Complex64[] ifftBuffer = new Complex64[frameSize];

            //A carrier that isn't on a bin isn't periodic in the frame: it jumps where the frame
            //wraps around, and Populate's truncated sideband series smears that jump into a
            //transient on both sides of the wrap. Reading the output from the middle of the unused
            //part of the frame keeps it away from the wrap. Each carrier is rotated so that it
            //still has its specified phase at the first output sample. On-bin carriers are
            //periodic in the frame, so their samples are unchanged.
            int offset = (frameSize - Samples.Length) / 2;
            double timeShift = -offset / (double)SamplingRate;

            foreach (ComplexCarrierTone carrierTone in carrierTones)
            {
                FrequencyDomain.Populate(ifftBuffer, carrierTone.TimeShift(timeShift));
            }

            Fourier.Inverse(ifftBuffer);

            for (int i = 0; i < Samples.Length; i++)
            {
                Samples[i] = (float)(outputScalar * ifftBuffer[offset + i].Real);
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

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        if (!initialized)
                        {
                            Initialize();
                        }
                        channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        //Only carriers that Populate actually renders into the frame
                        int frameSize = Samples.Length.CeilingToPowerOfTwo();
                        double rms = carrierTones
                            .Where(x => FrequencyDomain.IsRenderable(frameSize, x.frequency))
                            .Select(x => 0.5 * x.amplitude.MagnitudeSquared).Sum();
                        channelRMS = new double[] { Math.Sqrt(rms) };
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return channelRMS;
        }

        private readonly IEnumerable<PresentationConstraints> presentationConstraints = new PresentationConstraints[1] { null };
        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;
    }
}

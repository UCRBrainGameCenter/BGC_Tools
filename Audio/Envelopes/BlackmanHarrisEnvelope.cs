using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Envelopes
{
    /// <summary>
    /// A buffered Blackman-Harris Window Envelope.
    /// This class buffers the window because it's reused *frequently* in every case it's currently used at all.
    /// </summary>
    public class BlackmanHarrisEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;

        private readonly float[] samples;

        public BlackmanHarrisEnvelope(double duration)
        {
            Samples = (int)Math.Ceiling(duration * SamplingRate);

            samples = new float[Samples];
        }

        public BlackmanHarrisEnvelope(int samples)
        {
            Samples = samples;

            this.samples = new float[Samples];
        }

        protected override void _Initialize()
        {
            double a1Arg = 2 * Math.PI / (Samples - 1);
            double a2Arg = 2 * a1Arg;
            double a3Arg = 3 * a1Arg;

            const double a0 = 0.35875f;
            const double a1 = 0.48829f;
            const double a2 = 0.14128f;
            const double a3 = 0.01168f;

            for (int i = 0; i < Samples / 2; i++)
            {
                samples[i] = (float)(a0 - a1 * Math.Cos(i * a1Arg) + a2 * Math.Cos(i * a2Arg) - a3 * Math.Cos(i * a3Arg));
                samples[Samples - i - 1] = samples[i];
            }

            //Odd number of samples
            if (Samples % 2 == 1)
            {
                samples[Samples / 2 + 1] = 1f;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesToReturn = Math.Min(count, Samples - position);

            for (int i = 0; i < samplesToReturn; i++)
            {
                data[offset + i] = samples[position + i];
            }

            position += samplesToReturn;

            return samplesToReturn;
        }

        public override void Reset()
        {
            position = 0;
        }

        public override void Seek(int position)
        {
            this.position = position;
        }

        public override bool HasMoreSamples() => position < Samples;

        public override float ReadNextSample()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (HasMoreSamples())
            {
                return samples[position++];
            }

            return 0f;
        }
    }
}

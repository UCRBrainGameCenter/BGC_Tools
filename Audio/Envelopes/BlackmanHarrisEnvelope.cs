using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Envelopes
{
    public class BlackmanHarrisEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;

        private readonly double a1Arg;
        private readonly double a2Arg;
        private readonly double a3Arg;

        const double a0 = 0.35875f;
        const double a1 = 0.48829f;
        const double a2 = 0.14128f;
        const double a3 = 0.01168f;

        public BlackmanHarrisEnvelope(double duration)
        {
            Samples = (int)Math.Ceiling(duration * SamplingRate);

            a1Arg = 2 * Math.PI / (Samples - 1);
            a2Arg = 2 * a1Arg;
            a3Arg = 3 * a1Arg;
        }

        public BlackmanHarrisEnvelope(int samples)
        {
            Samples = samples;

            a1Arg = 2 * Math.PI / (Samples - 1);
            a2Arg = 2 * a1Arg;
            a3Arg = 3 * a1Arg;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToReturn = Math.Min(count, Samples - position);
            int tempSample;

            for (int i = 0; i < samplesToReturn; i++)
            {
                tempSample = position + i;
                data[offset + i] = (float)(a0 - a1 * Math.Cos(tempSample * a1Arg) + a2 * Math.Cos(tempSample * a2Arg) - a3 * Math.Cos(tempSample * a3Arg));
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
            if (HasMoreSamples())
            {
                int tempSample = position++;
                return (float)(a0 - a1 * Math.Cos(tempSample * a1Arg) + a2 * Math.Cos(tempSample * a2Arg) - a3 * Math.Cos(tempSample * a3Arg));
            }

            return 0f;
        }
    }
}

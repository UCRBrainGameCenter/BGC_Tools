using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Envelopes
{
    public class CosineEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;

        private readonly double cosineArgument;
        private readonly double alpha;
        private readonly double beta;
        private readonly double phase;

        private CosineEnvelope(double duration, bool open, double alpha)
        {
            Samples = (int)Math.Ceiling(duration * SamplingRate);
            cosineArgument = Math.PI / (Samples - 1);

            this.alpha = alpha;
            beta = 1.0 - alpha;

            if (open)
            {
                phase = 0.0;
            }
            else
            {
                phase = Math.PI;
            }
        }

        private CosineEnvelope(int samples, bool open, double alpha)
        {
            Samples = samples;
            cosineArgument = Math.PI / (samples - 1);

            this.alpha = alpha;
            beta = 1.0 - alpha;

            if (open)
            {
                phase = 0.0;
            }
            else
            {
                phase = Math.PI;
            }
        }

        public static CosineEnvelope HannWindow(double duration, bool open) =>
            new CosineEnvelope(duration, open, 0.5);

        public static CosineEnvelope HannWindow(int samples, bool open) =>
            new CosineEnvelope(samples, open, 0.5);

        public static CosineEnvelope HammingWindow(double duration, bool open) =>
            new CosineEnvelope(duration, open, 0.54);

        public static CosineEnvelope HammingWindow(int samples, bool open) =>
            new CosineEnvelope(samples, open, 0.54);

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToReturn = Math.Min(count, Samples - position);

            for (int i = 0; i < samplesToReturn; i++)
            {
                data[offset + i] = (float)(alpha - beta * Math.Cos((position + i) * cosineArgument + phase));
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
                return (float)(alpha - beta * Math.Cos(position++ * cosineArgument + phase));
            }

            return 0f;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.Audio.Envelopes
{
    public class ConstantEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;
        private readonly float value;

        public ConstantEnvelope(double value)
        {
            Debug.Assert(value >= -1.0 && value <= 1.0);

            this.value = (float)value;

            Samples = int.MaxValue;
        }

        public ConstantEnvelope(double value, double duration)
        {
            Debug.Assert(value >= -1.0 && value <= 1.0);

            this.value = (float)value;

            Samples = (int)Math.Ceiling(duration * SamplingRate);
        }

        public ConstantEnvelope(double value, int samples)
        {
            Debug.Assert(value >= -1.0 && value <= 1.0);

            this.value = (float)value;

            Samples = samples;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToReturn = Math.Min(count, Samples - position);

            for (int i = 0; i < samplesToReturn; i++)
            {
                data[offset + i] = value;
            }

            position += samplesToReturn;

            return samplesToReturn;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) => this.position = position;

        public override bool HasMoreSamples() => position < Samples;

        public override float ReadNextSample()
        {
            if (HasMoreSamples())
            {
                position++;
                return value;
            }

            return 0f;
        }
    }
}

using System;
using BGC.Mathematics;

namespace BGC.Audio.Envelopes
{
    public class SigmoidEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;
        private readonly double tanhLimit;

        public SigmoidEnvelope(
            double duration,
            double tanhLimit = 1.0)
        {
            this.tanhLimit = tanhLimit;

            Samples = (int)Math.Ceiling(duration * SamplingRate);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToReturn = Math.Min(count, Samples - position);

            for (int i = 0; i < samplesToReturn; i++)
            {
                double effectivePos = (position + i) / (0.5 * Samples) - 1.0;

                data[offset + i] = (float)GeneralMath.Tanh(effectivePos * tanhLimit);
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
                double effectivePos = position++ / (0.5f * Samples) - 1.0f;
                return (float)GeneralMath.Tanh(effectivePos * tanhLimit);
            }

            return 0f;
        }
    }
}

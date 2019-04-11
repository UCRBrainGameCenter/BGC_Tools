using System;
using System.Collections.Generic;

namespace BGC.Audio.Envelopes
{
    public class LinearEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private int position = 0;

        private readonly int growStartSample;
        private readonly int growEndSample;
        private readonly double invGrowHalfRange;

        public LinearEnvelope(double duration)
        {
            Samples = (int)Math.Ceiling(duration * SamplingRate);
            growStartSample = 0;
            growEndSample = Samples;
            invGrowHalfRange = 2.0 / (growEndSample - growStartSample);
        }

        public LinearEnvelope(double growthStartTime, double growthEndTime)
        {
            Samples = int.MaxValue;
            growStartSample = (int)Math.Ceiling(growthStartTime * SamplingRate);
            growEndSample = (int)Math.Ceiling(growthEndTime * SamplingRate);
            invGrowHalfRange = 2.0 / (growEndSample - growStartSample);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = Math.Min(count, Samples - position);

            while (samplesRemaining > 0)
            {
                if (position < growStartSample)
                {
                    int preReturnSamples = Math.Min(growStartSample - position, samplesRemaining);

                    for (int i = 0; i < preReturnSamples; i++)
                    {
                        data[offset + i] = -1f;
                    }

                    offset += preReturnSamples;
                    position += preReturnSamples;
                    samplesRemaining -= preReturnSamples;
                }
                else if (position < growEndSample)
                {
                    int growSamples = Math.Min(growEndSample - position, samplesRemaining);

                    for (int i = 0; i < growSamples; i++)
                    {
                        data[offset + i] = (float)((position + i - growStartSample) * invGrowHalfRange - 1.0);
                    }

                    offset += growSamples;
                    position += growSamples;
                    samplesRemaining -= growSamples;
                }
                else
                {
                    int postReturnSamples = Math.Min(Samples - position, samplesRemaining);

                    if (postReturnSamples <= 0)
                    {
                        break;
                    }

                    for (int i = 0; i < postReturnSamples; i++)
                    {
                        data[offset + i] = 1f;
                    }

                    offset += postReturnSamples;
                    position += postReturnSamples;
                    samplesRemaining -= postReturnSamples;

                }
            }

            return count - samplesRemaining;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) => this.position = position;

        public override bool HasMoreSamples() => position < Samples;

        public override float ReadNextSample()
        {
            if (!HasMoreSamples())
            {
                return 0f;
            }

            if (position < growStartSample)
            {
                position++;
                return -1f;
            }

            if (position < growEndSample)
            {
                return (float)((position++ - growStartSample) * invGrowHalfRange - 1.0);
            }

            position++;
            return 1f;
        }
    }
}

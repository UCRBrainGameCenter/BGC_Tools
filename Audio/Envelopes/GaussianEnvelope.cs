using System;
using UnityEngine;

namespace BGC.Audio.Envelopes
{
    public class GaussianEnvelope : BGCEnvelopeStream
    {
        public override float SamplingRate => 44100f;

        public override int Samples { get; }

        private readonly double expOffset;
        private readonly double expFactor;
        private readonly double ampFactor;
        private readonly double peakSample;

        private readonly int startEnvelopeSample;
        private readonly int endEnvelopeSample;

        private int position = 0;

        public GaussianEnvelope(
            double gaussianWidth,
            double peakTime = double.NaN,
            double sigmaFloor = 3.0)
        {
            expOffset = Math.Exp(-sigmaFloor * sigmaFloor / 2.0);
            expFactor = -(2.0 * sigmaFloor * sigmaFloor) / (gaussianWidth * gaussianWidth * SamplingRate * SamplingRate);
            ampFactor = 1.0 / (1.0 - expOffset);

            if (double.IsNaN(peakTime))
            {
                //Place peak at the center of the gaussian
                peakTime = gaussianWidth / 2.0;
                startEnvelopeSample = 0;
            }
            else
            {
                //Ensure we have enough time to reach the peak
                Debug.Assert(peakTime >= gaussianWidth / 2.0);
                startEnvelopeSample = (int)Math.Floor((peakTime - gaussianWidth / 2.0) * SamplingRate);
            }

            peakSample = peakTime * SamplingRate;
            endEnvelopeSample = (int)Math.Ceiling(peakSample + gaussianWidth * SamplingRate / 2.0);
            Samples = endEnvelopeSample;
        }

        public GaussianEnvelope(
            double totalDuration,
            double gaussianWidth,
            double peakTime = double.NaN,
            double sigmaFloor = 3.0)
        {
            Debug.Assert(totalDuration >= gaussianWidth,
                "The total duration must be at least large enough to support the Gaussian specified");

            expOffset = Math.Exp(-sigmaFloor * sigmaFloor / 2.0);
            expFactor = -(2.0 * sigmaFloor * sigmaFloor) / (gaussianWidth * gaussianWidth * SamplingRate * SamplingRate);
            ampFactor = 1.0 / (1.0 - expOffset);

            if (double.IsNaN(peakTime))
            {
                //Place peak at the center of the whole window
                peakTime = totalDuration / 2.0;
            }
            else
            {
                //Ensure we have enough time to reach the peak
                Debug.Assert(peakTime >= gaussianWidth / 2.0);
                Debug.Assert(totalDuration >= peakTime + gaussianWidth / 2.0,
                    "The total duration must be at least large enough to support the Gaussian specified");
            }

            startEnvelopeSample = (int)Math.Floor((peakTime - gaussianWidth / 2.0) * SamplingRate);
            peakSample = peakTime * SamplingRate;
            endEnvelopeSample = (int)Math.Ceiling(peakSample + gaussianWidth * SamplingRate / 2.0);
            Samples = (int)Math.Ceiling(totalDuration * SamplingRate);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                if (position < startEnvelopeSample)
                {
                    //Prior to envelope - Return 0
                    int samplesHandled = Math.Min(startEnvelopeSample - position, samplesRemaining);

                    for (int i = 0; i < samplesHandled; i++)
                    {
                        data[offset + i] = 0f;
                    }

                    offset += samplesHandled;
                    samplesRemaining -= samplesHandled;
                    position += samplesHandled;
                }
                else if (position < endEnvelopeSample)
                {
                    //Inside Envelope
                    int samplesHandled = Math.Min(endEnvelopeSample - position, samplesRemaining);
                    double positionValue;
                    for (int i = 0; i < samplesHandled; i++)
                    {
                        positionValue = position + i - peakSample;

                        data[offset + i] = (float)(ampFactor *
                            (Math.Exp(expFactor * positionValue * positionValue) - expOffset));
                    }

                    offset += samplesHandled;
                    samplesRemaining -= samplesHandled;
                    position += samplesHandled;
                }
                else
                {
                    //After envelope - Return 0
                    int samplesHandled = Math.Min(Samples - position, samplesRemaining);

                    if (samplesHandled <= 0)
                    {
                        break;
                    }

                    for (int i = 0; i < samplesHandled; i++)
                    {
                        data[offset + i] = 0f;
                    }

                    offset += samplesHandled;
                    samplesRemaining -= samplesHandled;
                    position += samplesHandled;
                }
            }

            return count - samplesRemaining;
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
                if (position < startEnvelopeSample)
                {
                    //Prior to envelope - Return 0
                    position++;
                    return 0f;
                }
                else if (position < endEnvelopeSample)
                {
                    double positionValue = position++ - peakSample;

                    return (float)(ampFactor *
                        (Math.Exp(expFactor * positionValue * positionValue) - expOffset));
                }
                else
                {
                    //After envelope - Return 0
                    position++;
                    return 0f;
                }
            }

            return 0f;
        }
    }
}

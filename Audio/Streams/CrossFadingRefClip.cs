using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Holds a reference to a large underlying sample buffer.
    /// Loops and crossfades with itself.
    /// Used for managing extra-large TENoise sample.
    /// </summary>
    public class CrossFadingRefClip : BGCAudioClip
    {
        //Masquerading as a 2-channel clip
        public override int Channels => 2;

        public override int TotalSamples => Channels * Samples.Length;
        public override int ChannelSamples => Samples.Length;

        public float[] Samples { get; }

        public int Position { get; private set; }

        private readonly float leftFactor = 0f;
        private readonly float rightFactor = 0f;

        private const float FADE_DURATION = 1.0f;

        private readonly float[] window = null;
        private int initialWindowStart = -1;
        private readonly int startCrossFade;

        public CrossFadingRefClip(
            float[] samples,
            double leftFactor,
            double rightFactor,
            int initialPosition = -1)
        {
            Samples = samples;

            this.leftFactor = (float)leftFactor;
            this.rightFactor = (float)rightFactor;

            window = new float[(int)(SamplingRate * FADE_DURATION)];

            double sineArgument = Math.PI / (2 * window.Length - 1);

            for (int i = 0; i < window.Length; i++)
            {
                window[i] = (float)Math.Sin(i * sineArgument);
            }
            
            startCrossFade = Samples.Length - window.Length;

            if (initialPosition == -1)
            {
                //Start at least 2 window lengths before end
                initialPosition = CustomRandom.Next(0, Samples.Length - (2 * window.Length + 2));
            }

            Position = initialPosition;
            initialWindowStart = Position;
        }

        public override void Reset()
        {
            //Start at least 2 window lengths before end
            Position = CustomRandom.Next(0, Samples.Length - (2 * window.Length + 2));
            initialWindowStart = Position;
        }

        public override int Read(float[] data, int offset, int count)
        {
            //Split into regions
            int remainingCount = count;

            while (remainingCount > 0)
            {
                if (initialWindowStart != -1)
                {
                    //Initial window period

                    //Windowing region

                    int initialWindowingEndSample = initialWindowStart + window.Length;

                    int copyLength = Math.Min(initialWindowingEndSample - Position, remainingCount / 2);

                    for (int i = 0; i < copyLength; i++)
                    {
                        float sample = window[Position - initialWindowStart + i] * Samples[Position + i];
                        data[offset + 2 * i] = leftFactor * sample;
                        data[offset + 2 * i + 1] = rightFactor * sample;
                    }

                    //Advance Indices
                    Position += copyLength;
                    offset += 2 * copyLength;
                    remainingCount -= 2 * copyLength;

                    if (Position == initialWindowingEndSample)
                    {
                        //Done with initial window
                        initialWindowStart = -1;
                    }

                }
                else if (Position < startCrossFade)
                {
                    //Region 1 - No windowing Needed
                    int copyLength = Math.Min(startCrossFade - Position, remainingCount / 2);

                    for (int i = 0; i < copyLength; i++)
                    {
                        data[offset + 2 * i] = leftFactor * Samples[Position + i];
                        data[offset + 2 * i + 1] = rightFactor * Samples[Position + i];
                    }

                    //Advance Indices
                    Position += copyLength;
                    offset += 2 * copyLength;
                    remainingCount -= 2 * copyLength;
                }
                else
                {
                    //Region 2 - Cross-windowing
                    int copyLength = Math.Min(Samples.Length - Position, remainingCount / 2);

                    for (int i = 0; i < copyLength; i++)
                    {
                        float outSample = window[Samples.Length - Position - i - 1] * Samples[Position + i];
                        float inSample = window[Position + i - startCrossFade] * Samples[Position + i - startCrossFade];
                        data[offset + 2 * i] = leftFactor * (outSample + inSample);
                        data[offset + 2 * i + 1] = rightFactor * (outSample + inSample);
                    }

                    //Advance Indices
                    Position += copyLength;
                    offset += 2 * copyLength;
                    remainingCount -= 2 * copyLength;

                    if (Position >= Samples.Length)
                    {
                        Position = window.Length;
                    }
                }
            }

            return count;
        }

        public override void Seek(int position) => 
            Position = GeneralMath.Clamp(position, 0, Samples.Length - (2 * window.Length + 2));

        public override IEnumerable<double> GetChannelRMS() => throw new NotSupportedException();
    }
}

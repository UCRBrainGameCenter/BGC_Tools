using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// True Continuous White-Noise stream
    /// </summary>
    public class ContinuousWhiteNoiseStream : BGCAudioClip
    {
        public override int Channels => 1;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly Random randomizer;

        private readonly double rms;
        private readonly double factor;

        public ContinuousWhiteNoiseStream(
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            rms = 1.0 / Math.Sqrt(3.0);
            factor = 1.0;
        }

        public ContinuousWhiteNoiseStream(
            double rms,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            this.rms = rms;
            factor = rms * Math.Sqrt(3.0);
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            //Read...

            for (int i = 0; i < count; i++)
            {
                data[offset + i] = (float)(factor * (1.0 - 2.0 * randomizer.NextDouble()));
            }

            return count;
        }

        public override void Reset() { }

        public override void Seek(int position) { }

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return rms;
        }
    }
}

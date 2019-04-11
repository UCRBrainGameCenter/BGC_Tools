using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// True White-Noise stream
    /// </summary>
    public class WhiteNoiseStream : BGCAudioClip
    {
        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        private int _channelSamples;
        public override int ChannelSamples => _channelSamples;

        private readonly Random randomizer;

        private readonly double rms;
        private readonly float[] samples;

        private int position = 0;

        public WhiteNoiseStream(
            double duration,
            double rms,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            this.rms = rms;

            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
            samples = new float[_channelSamples];
        }

        protected override void _Initialize()
        {
            double currentRMS = 0.0;

            for (int i = 0; i < _channelSamples; i++)
            {
                samples[i] = randomizer.NextFloat();
                currentRMS += samples[i] * samples[i];
            }

            currentRMS = Math.Sqrt(currentRMS / _channelSamples);

            float factor = (float)(rms / currentRMS);

            for (int i = 0; i < _channelSamples; i++)
            {
                samples[i] *= factor;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            //Read...

            int samplesToRead = Math.Min(count, _channelSamples - position);

            Array.Copy(
                sourceArray: samples,
                sourceIndex: position,
                destinationArray: data,
                destinationIndex: offset,
                length: samplesToRead);

            position += samplesToRead;

            return samplesToRead;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) => 
            this.position = GeneralMath.Clamp(position, 0, _channelSamples);

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return rms;
        }
    }
}

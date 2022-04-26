using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Continuous, linear tone sweep from starting to ending frequency over 
    /// specified duration.
    /// </summary>
    public class SweepAudioClip : BGCAudioClip
    {
        public enum SweepDirection
        {
            Up = 0,
            Down,
            MAX
        }

        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        private readonly int _channelSamples;
        public override int ChannelSamples => _channelSamples;

        private readonly Random randomizer;

        public readonly SweepDirection sweepDirection;
        private readonly double freqLB;
        private readonly double freqUB;
        private float[] samples = null;

        private int position = 0;

        public SweepAudioClip(
            double duration,
            double freqLB,
            double freqUB,
            SweepDirection sweepDirection,
            Random randomizer = null)
        {
            if (randomizer == null)
            {
                randomizer = new Random(CustomRandom.Next());
            }
            this.randomizer = randomizer;

            this.freqLB = freqLB;
            this.freqUB = freqUB;
            this.sweepDirection = sweepDirection;

            _channelSamples = (int)Math.Ceiling(duration * SamplingRate);
        }

        protected override void _Initialize()
        {
            samples = new float[_channelSamples];

            double startingPhase = 2.0 * Math.PI * randomizer.NextDouble();

            double startingFreq;
            double endingFreq;

            switch (sweepDirection)
            {
                case SweepDirection.Up:
                    startingFreq = freqLB;
                    endingFreq = freqUB;
                    break;

                case SweepDirection.Down:
                    startingFreq = freqUB;
                    endingFreq = freqLB;
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected SweepDirection: {sweepDirection}");
                    return;
            }

            double freqRatio = endingFreq / startingFreq;
            double freqTerm = 2.0 * Math.PI * startingFreq * samples.Length / (SamplingRate * Math.Log(freqRatio));

            //Intercept 0 change
            if (startingFreq == endingFreq)
            {
                freqTerm = 2.0 * Math.PI * startingFreq / SamplingRate;

                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] = (float)Math.Sin(startingPhase + freqTerm * i);
                }

                return;
            }

            //General Case
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)Math.Sin(startingPhase +
                    freqTerm * (Math.Pow(freqRatio, i / (double)samples.Length) - 1.0));
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


        private readonly IEnumerable<double> channelRMS = new double[] { Math.Sqrt(0.5) };
        public override IEnumerable<double> GetChannelRMS() => channelRMS;

        private readonly IEnumerable<PresentationConstraints> presentationConstraints = new PresentationConstraints[1] { null };
        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;

    }
}

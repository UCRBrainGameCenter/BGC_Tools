using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Stream that stores a mono sample buffer and scaling factors.
    /// Plays as a stereo stream.
    /// </summary>
    public class NormalizedMonoClip : BGCAudioClip
    {
        public int Position { get; set; }

        public override int TotalSamples => 2 * Samples.Length;
        public override int ChannelSamples => Samples.Length;

        public float[] Samples { get; }

        public override int Channels => 2;

        private int RemainingChannelSamples => Math.Max(0, Samples.Length - Position);
        private int RemainingTotalSamples => 2 * RemainingChannelSamples;

        private bool factorsInitialized = false;
        private float leftFactor;
        private float rightFactor;
        private readonly double presentationLevel;

        public NormalizedMonoClip(float[] monoSamples, double leftFactor, double rightFactor)
        {
            Samples = monoSamples;
            this.leftFactor = (float)leftFactor;
            this.rightFactor = (float)rightFactor;
            factorsInitialized = true;
        }

        public NormalizedMonoClip(float[] monoSamples, double presentationLevel)
        {
            Samples = monoSamples;
            this.presentationLevel = presentationLevel;
            factorsInitialized = false;
        }

        protected override void _Initialize()
        {
            if (!factorsInitialized)
            {
                factorsInitialized = true;

                Normalization.GetMonoRMSScalingFactors(
                    monoSamples: Samples,
                    desiredLevel: presentationLevel,
                    scalingFactorL: out double tempLeftFactor,
                    scalingFactorR: out double tempRightFactor);

                leftFactor = (float)tempLeftFactor;
                rightFactor = (float)tempRightFactor;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int copyLength = Math.Min(RemainingChannelSamples, count / 2);

            for (int i = 0; i < copyLength; i++)
            {
                data[offset + 2 * i] = leftFactor * Samples[Position + i];
                data[offset + 2 * i + 1] = rightFactor * Samples[Position + i];
            }

            Position += copyLength;

            return 2 * copyLength;
        }

        public override void Seek(int position) => Position = GeneralMath.Clamp(position, 0, ChannelSamples);

        public override void Reset() => Position = 0;

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double rms = 0.0;

                for (int i = 0; i < ChannelSamples; i++)
                {
                    rms += Samples[i] * Samples[i];
                }

                rms = Math.Sqrt(rms / ChannelSamples);

                _channelRMS = new double[2] { leftFactor * rms, rightFactor * rms };
            }

            return _channelRMS;
        }
    }
}

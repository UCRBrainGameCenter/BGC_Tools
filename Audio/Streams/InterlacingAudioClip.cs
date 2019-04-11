using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Stream that stores the left and right channels as separate arrays
    /// </summary>
    public class InterlacingAudioClip : BGCAudioClip
    {
        public int Position { get; private set; } = 0;

        /// <summary>
        /// Simulated raw sample position
        /// </summary>
        public int RawPosition
        {
            get => Position * Channels;
            set => Position = value / Channels;
        }

        public override int ChannelSamples => LSamples.Length;
        public override int TotalSamples => Channels * LSamples.Length;

        public float[] LSamples { get; }
        public float[] RSamples { get; }

        public override int Channels => 2;

        private int RemainingChannelSamples => Math.Max(0, LSamples.Length - Position);
        private int RemainingTotalSamples => Channels * RemainingChannelSamples;

        public InterlacingAudioClip(float[] leftSamples, float[] rightSamples)
        {
            LSamples = leftSamples;
            RSamples = rightSamples;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int copyLength = Math.Min(RemainingTotalSamples, count);

            for (int i = 0; i < copyLength / 2; i++)
            {
                data[offset + 2 * i] = LSamples[Position + i];
                data[offset + 2 * i + 1] = RSamples[Position + i];
            }

            RawPosition += copyLength;

            return copyLength;
        }

        public override void Seek(int position) =>
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);

        public override void Reset() => Position = 0;

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double rmsL = 0.0;
                double rmsR = 0.0;

                for (int i = 0; i < ChannelSamples; i++)
                {
                    rmsL += LSamples[i] * LSamples[i];
                    rmsR += RSamples[i] * RSamples[i];
                }

                rmsL = Math.Sqrt(rmsL / ChannelSamples);
                rmsR = Math.Sqrt(rmsR / ChannelSamples);

                _channelRMS = new double[2] { rmsL, rmsR };
            }

            return _channelRMS;
        }
    }
}

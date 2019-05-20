using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Truncates underlying stream
    /// </summary>
    public class StreamTruncator : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;

        public override int TotalSamples => Channels * ChannelSamples;
        public override int ChannelSamples { get; }

        private readonly int sampleOffset;

        private readonly bool recalculateRMS;

        public int Position { get; private set; }

        public StreamTruncator(
            IBGCStream stream,
            int samples,
            int sampleOffset = 0,
            bool recalculateRMS = false)
            : base(stream)
        {
            if (sampleOffset > stream.ChannelSamples)
            {
                Debug.LogError("Requested a sampleOffset larger than clip length");
                sampleOffset = 0;
            }

            this.sampleOffset = sampleOffset;

            ChannelSamples = Math.Min(samples, stream.ChannelSamples - sampleOffset);

            this.recalculateRMS = recalculateRMS;

            Reset();
        }

        public StreamTruncator(
            IBGCStream stream,
            double totalDuration = double.NaN,
            int sampleOffset = 0,
            bool recalculateRMS = false)
            : base(stream)
        {
            if (sampleOffset > stream.ChannelSamples)
            {
                Debug.LogError("Requested a sampleOffset larger than clip length");
                sampleOffset = 0;
            }

            this.sampleOffset = sampleOffset;

            if (!double.IsNaN(totalDuration))
            {
                ChannelSamples = Math.Min(
                    (int)(totalDuration * SamplingRate),
                    stream.ChannelSamples - sampleOffset);
            }
            else
            {
                ChannelSamples = stream.ChannelSamples - sampleOffset;
            }

            this.recalculateRMS = recalculateRMS;

            Reset();
        }

        public override void Reset()
        {
            Position = 0;
            stream.Reset();
            if (sampleOffset > 0)
            {
                stream.Seek(sampleOffset);
            }
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);
            stream.Seek(Position + sampleOffset);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0 && Position < ChannelSamples)
            {
                int copyLength = Math.Min(Channels * Position, remainingSamples);
                int readSamples = stream.Read(data, offset, copyLength);

                if (readSamples == 0)
                {
                    //No more samples
                    break;
                }

                remainingSamples -= readSamples;
                offset += readSamples;
                Position += readSamples / Channels;
            }

            return count - remainingSamples;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                if (recalculateRMS)
                {
                    _channelRMS = this.CalculateRMS();
                }
                else
                {
                    _channelRMS = stream.GetChannelRMS();
                }
            }

            return _channelRMS;
        }
    }

}

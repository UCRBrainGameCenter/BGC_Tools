using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Shifts time forward or backwards (integer samples) of the underlying stream
    /// </summary>
    public class StreamTimeShiftFilter : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        private readonly int sampleOffset;

        private int position = 0;

        public StreamTimeShiftFilter(
            IBGCStream stream,
            double timeShift)
            : base(stream)
        {
            sampleOffset = (int)Math.Round(timeShift * SamplingRate);

            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = stream.ChannelSamples - sampleOffset;
                TotalSamples = Channels * ChannelSamples;
            }
        }

        public StreamTimeShiftFilter(
            IBGCStream stream,
            int sampleShift)
            : base(stream)
        {
            sampleOffset = sampleShift;

            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = stream.ChannelSamples - sampleOffset;
                TotalSamples = Channels * ChannelSamples;
            }
        }

        public override void Reset()
        {
            position = 0;
            stream.Reset();

            if (sampleOffset > 0)
            {
                stream.Seek(sampleOffset);
            }
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, ChannelSamples);

            if (position + sampleOffset > 0)
            {
                stream.Seek(position + sampleOffset);
            }
            else
            {
                stream.Reset();
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0)
            {
                if (position < -sampleOffset)
                {
                    //Initial Dead Zone

                    int deadSampleLength = Math.Min(
                        Channels * (-sampleOffset - position),
                        remainingSamples);

                    for (int i = 0; i < deadSampleLength; i++)
                    {
                        data[offset + i] = 0f;
                    }

                    remainingSamples -= deadSampleLength;
                    offset += deadSampleLength;
                    position += deadSampleLength / Channels;
                }
                else
                {
                    //Read Zone
                    int copyLength = Math.Min(
                        Channels * (ChannelSamples - position),
                        remainingSamples);

                    int readSamples = stream.Read(data, offset, copyLength);

                    if (readSamples == 0)
                    {
                        //No more samples
                        break;
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    position += readSamples / Channels;
                }
            }

            return count - remainingSamples;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = stream.GetChannelRMS();
            }

            return _channelRMS;
        }
    }

}

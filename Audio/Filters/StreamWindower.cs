using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Windows and truncates underlying stream
    /// </summary>
    public class StreamWindower : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;

        public override int TotalSamples => Channels * ChannelSamples;
        public override int ChannelSamples { get; }

        private readonly float[] window;
        private readonly int sampleOffset;

        private readonly int endOpeningWindow;
        private readonly int startClosingWindow;

        private readonly bool recalculateRMS;

        public int Position { get; private set; }

        public StreamWindower(
            IBGCStream stream,
            Windowing.Function function = Windowing.Function.Hamming,
            double totalDuration = double.NaN,
            int smoothingSamples = 1000,
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

            smoothingSamples = Math.Min(smoothingSamples, ChannelSamples / 2);

            window = Windowing.GetHalfWindow(function, smoothingSamples);

            endOpeningWindow = smoothingSamples;
            startClosingWindow = ChannelSamples - smoothingSamples;

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
                if (Position < endOpeningWindow)
                {
                    //Initial Window Period

                    int copyLength = Math.Min(
                        Channels * (endOpeningWindow - Position),
                        remainingSamples);

                    int readSamples = stream.Read(data, offset, copyLength);

                    if (readSamples == 0)
                    {
                        //No more samples
                        break;
                    }

                    for (int i = 0; i < readSamples; i++)
                    {
                        data[offset + i] *= window[Position + (i / Channels)];
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples / Channels;
                }
                else if (Position < startClosingWindow)
                {
                    //Unwindowed Period

                    int copyLength = Math.Min(
                        Channels * (startClosingWindow - Position),
                        remainingSamples);

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
                else
                {
                    //Ending Window Period

                    int copyLength = Math.Min(
                        Channels * (ChannelSamples - Position),
                        remainingSamples);

                    int readSamples = stream.Read(data, offset, copyLength);

                    if (readSamples == 0)
                    {
                        //No more samples
                        break;
                    }

                    for (int i = 0; i < readSamples; i++)
                    {
                        data[offset + i] *= window[window.Length + startClosingWindow - (Position + (i / Channels) + 1)];
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples / Channels;
                }
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

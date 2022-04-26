using System;
using System.Collections.Generic;
using System.Linq;
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

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        private readonly float[] openingWindow;
        private readonly float[] closingWindow;
        private readonly int sampleShift;

        private readonly TransformRMSBehavior rmsBehavior;

        private readonly int endOpeningWindow;
        private readonly int startClosingWindow;

        public int Position { get; private set; }

        public StreamWindower(
            IBGCStream stream,
            Windowing.Function function = Windowing.Function.Hamming,
            double totalDuration = double.NaN,
            int smoothingSamples = 1000,
            int sampleShift = 0,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (sampleShift > stream.ChannelSamples)
            {
                Debug.LogError("Requested a sampleOffset larger than clip length");
                sampleShift = 0;
            }

            this.sampleShift = sampleShift;

            if (!double.IsNaN(totalDuration))
            {
                ChannelSamples = Math.Min(
                    (int)Math.Round(totalDuration * SamplingRate),
                    stream.ChannelSamples - sampleShift);
                TotalSamples = Channels * ChannelSamples;
            }
            else
            {
                if (stream.ChannelSamples == int.MaxValue)
                {
                    ChannelSamples = int.MaxValue;
                    TotalSamples = int.MaxValue;
                }
                else
                {
                    ChannelSamples = stream.ChannelSamples - sampleShift;
                    TotalSamples = Channels * ChannelSamples;
                }
            }

            this.rmsBehavior = rmsBehavior;

            smoothingSamples = Math.Min(smoothingSamples, ChannelSamples / 2);

            openingWindow = Windowing.GetHalfWindow(function, smoothingSamples);
            closingWindow = openingWindow;

            endOpeningWindow = smoothingSamples;
            startClosingWindow = ChannelSamples - smoothingSamples;

            Reset();
        }

        public StreamWindower(
            IBGCStream stream,
            Windowing.Function openingFunction,
            Windowing.Function closingFunction,
            int openingSmoothingSamples = 1000,
            int closingSmoothingSamples = 1000,
            int sampleShift = 0,
            int totalChannelSamples = -1,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (sampleShift > stream.ChannelSamples)
            {
                Debug.LogError("Requested a sampleOffset larger than clip length");
                sampleShift = 0;
            }

            this.sampleShift = sampleShift;
            this.rmsBehavior = rmsBehavior;

            if (totalChannelSamples != -1)
            {
                ChannelSamples = Math.Min(
                    totalChannelSamples,
                    stream.ChannelSamples - sampleShift);
                TotalSamples = Channels * ChannelSamples;
            }
            else
            {
                if (stream.ChannelSamples == int.MaxValue)
                {
                    ChannelSamples = int.MaxValue;
                    TotalSamples = int.MaxValue;
                }
                else
                {
                    ChannelSamples = stream.ChannelSamples - sampleShift;
                    TotalSamples = Channels * ChannelSamples;
                }
            }

            if (openingSmoothingSamples + closingSmoothingSamples > ChannelSamples)
            {
                //Requested smoothing samples exceeded remaining stream length
                int totalSmoothingSamples = openingSmoothingSamples + closingSmoothingSamples;
                int excessSamples = ChannelSamples - totalSmoothingSamples;

                //Allocate reduced smoothing samples based on requested percentage
                openingSmoothingSamples -= (int)Math.Round(
                    excessSamples * (openingSmoothingSamples / (double)totalSmoothingSamples));
                closingSmoothingSamples = ChannelSamples - openingSmoothingSamples;
            }

            openingWindow = Windowing.GetHalfWindow(openingFunction, openingSmoothingSamples);
            closingWindow = Windowing.GetHalfWindow(closingFunction, closingSmoothingSamples);

            endOpeningWindow = openingSmoothingSamples;
            startClosingWindow = ChannelSamples - closingSmoothingSamples;

            Reset();
        }

        public override void Reset()
        {
            Position = 0;
            stream.Reset();
            if (sampleShift > 0)
            {
                stream.Seek(sampleShift);
            }
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);
            stream.Seek(Position + sampleShift);
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
                        data[offset + i] *= openingWindow[Position + (i / Channels)];
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
                        data[offset + i] *= closingWindow[closingWindow.Length + startClosingWindow - (Position + (i / Channels) + 1)];
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples / Channels;
                }
            }

            return count - remainingSamples;
        }

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        channelRMS = stream.GetChannelRMS();

                        if (channelRMS.Any(double.IsNaN) && ChannelSamples != int.MaxValue)
                        {
                            goto case TransformRMSBehavior.Recalculate;
                        }
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return channelRMS;
        }
    }
}

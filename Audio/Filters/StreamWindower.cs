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
    public class StreamWindower : StreamTruncator
    {
        protected float[] openingWindow;
        protected float[] closingWindow;

        protected int endOpeningWindow;
        protected int startClosingWindow;
        
        public StreamWindower(
            IBGCStream stream,
            Windowing.Function openingFunction,
            Windowing.Function closingFunction,
            int openingSmoothingSamples = 1000,
            int closingSmoothingSamples = 1000,
            bool randomStart = false,
            int offset = 0,
            double totalDuration = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream, randomStart, totalDuration, offset, rmsBehavior)
        {
            CalculateWindows(openingFunction, closingFunction, openingSmoothingSamples, closingSmoothingSamples);
        }
        
        public StreamWindower(
            IBGCStream stream,
            Windowing.Function openingFunction,
            Windowing.Function closingFunction,
            int openingSmoothingSamples = 1000,
            int closingSmoothingSamples = 1000,
            bool randomStart = false,
            int offset = 0,
            int totalChannelSamples = -1,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream, randomStart, totalChannelSamples, offset, rmsBehavior)
        {
            CalculateWindows(openingFunction, closingFunction, openingSmoothingSamples, closingSmoothingSamples);
        }

        private void CalculateWindows(Windowing.Function openingFunction, Windowing.Function closingFunction, int openingSmoothingSamples, int closingSmoothingSamples)
        {
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
    }
}

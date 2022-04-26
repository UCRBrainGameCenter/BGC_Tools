using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// A Decorator class for BGCAudioClips that windows and truncates underlying clip
    /// </summary>
    public class AnalyticStreamWindower : SimpleAnalyticFilter
    {
        public override int Samples { get; }

        private readonly double[] window;
        private readonly int sampleOffset;

        private readonly int endOpeningWindow;
        private readonly int startClosingWindow;

        private readonly TransformRMSBehavior rmsBehavior;

        public int Position { get; private set; }

        public AnalyticStreamWindower(
            IAnalyticStream stream,
            Windowing.Function function,
            double totalDuration = double.NaN,
            int smoothingSamples = 1000,
            int sampleOffset = 0,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (sampleOffset > stream.Samples)
            {
                Debug.LogError("Requested a sampleOffset larger than clip length");
                sampleOffset = 0;
            }

            this.sampleOffset = sampleOffset;

            if (!double.IsNaN(totalDuration))
            {
                Samples = Math.Min(
                    (int)(totalDuration * SamplingRate),
                    stream.Samples - sampleOffset);
            }
            else
            {
                Samples = stream.Samples - sampleOffset;
            }

            this.rmsBehavior = rmsBehavior;

            smoothingSamples = Math.Min(smoothingSamples, Samples / 2);

            window = Windowing.GetHalfWindow64(function, smoothingSamples);

            endOpeningWindow = smoothingSamples;
            startClosingWindow = Samples - smoothingSamples;

            Reset();
        }

        public override void Reset()
        {
            Position = 0;
            stream.Reset();
            stream.Seek(sampleOffset);
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, Samples);
            stream.Seek(Position + sampleOffset);
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0 && Position < Samples)
            {
                if (Position < endOpeningWindow)
                {
                    //Initial Window Period

                    int copyLength = Math.Min(remainingSamples, endOpeningWindow - Position);

                    int readSamples = stream.Read(data, offset, copyLength);

                    for (int i = 0; i < readSamples; i++)
                    {
                        data[offset + i] *= window[Position + i];
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples;
                }
                else if (Position < startClosingWindow)
                {
                    //Unwindowed Period

                    int copyLength = Math.Min(remainingSamples, startClosingWindow - Position);

                    int readSamples = stream.Read(data, offset, copyLength);

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples;
                }
                else
                {
                    //Ending Window Period

                    int copyLength = Math.Min(remainingSamples, Samples - Position);

                    int readSamples = stream.Read(data, offset, copyLength);

                    if (readSamples == 0)
                    {
                        //No more samples
                        break;
                    }

                    for (int i = 0; i < readSamples; i++)
                    {
                        data[offset + i] *= window[window.Length + startClosingWindow - (Position + i + 1)];
                    }

                    remainingSamples -= readSamples;
                    offset += readSamples;
                    Position += readSamples;
                }
            }

            return count - remainingSamples;
        }

        private double channelRMS = double.NaN;
        public override double GetRMS()
        {
            if (double.IsNaN(channelRMS))
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        channelRMS = stream.GetRMS();

                        if (double.IsNaN(channelRMS) && Samples != int.MaxValue)
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

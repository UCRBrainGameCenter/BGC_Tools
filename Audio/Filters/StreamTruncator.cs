using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Truncates underlying stream
    /// </summary>
    public class StreamTruncator : SimpleBGCFilter //TODO rename?
    {
        public override int Channels => stream.Channels;
        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }
        public int Position { get; protected set; }

        private int offset;
        private TransformRMSBehavior rmsBehavior;
        
        public StreamTruncator(
            IBGCStream stream,
            bool randomStart,
            int totalChannelSamples = -1,
            int offset = 0,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (totalChannelSamples > stream.ChannelSamples && stream.ChannelSamples != int.MaxValue)
            {
                Debug.LogError("Requested a duration larger than clip length");
                totalChannelSamples = stream.ChannelSamples;
            }
            else if (totalChannelSamples == -1)
            {
                totalChannelSamples = stream.ChannelSamples; // may be int.MaxValue
            }
            
            if (randomStart)
            {
                System.Random random = new System.Random();
                offset = (int)(random.NextDouble() * (stream.ChannelSamples - totalChannelSamples));
            }
            else if (offset > stream.ChannelSamples)
            {
                Debug.LogError("Requested an offset larger than clip length");
                offset = 0;
            }

            this.offset = offset;
            
            ChannelSamples = Math.Min(
                totalChannelSamples,
                stream.ChannelSamples - offset);
            TotalSamples = Channels * ChannelSamples;

            this.rmsBehavior = rmsBehavior;

            Reset();
        }
        
        public StreamTruncator(
            IBGCStream stream,
            bool randomStart,
            double totalDuration = double.NaN,
            int offset = 0,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            this.rmsBehavior = rmsBehavior;
            
            if (offset > stream.ChannelSamples)
            {
                Debug.LogError("Requested an offset larger than clip length");
                offset = 0;
            }
            
            this.offset = offset;

            if (!double.IsNaN(totalDuration))
            {
                double clipDuration = stream.Duration();
                if (totalDuration > clipDuration)
                {
                    Debug.LogError("Requested a duration larger than clip length");
                    totalDuration = clipDuration;
                }
                
                if (randomStart)
                {
                    System.Random random = new System.Random();
                    this.offset = (int)(random.NextDouble() * (stream.ChannelSamples - (totalDuration * SamplingRate)));
                }

                ChannelSamples = Math.Min(
                    (int)Math.Round(totalDuration * SamplingRate),
                    stream.ChannelSamples - this.offset);
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
                    ChannelSamples = stream.ChannelSamples - this.offset;
                    TotalSamples = Channels * ChannelSamples;
                }
            }
            
            Reset();
        }

        public override void Reset()
        {
            Position = 0;
            stream.Reset();
            if (offset > 0)
            {
                stream.Seek(offset);
            }
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);
            stream.Seek(Position + offset);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0 && Position < ChannelSamples)
            {
                int copyLength = Math.Min(Channels * (ChannelSamples - Position), remainingSamples);
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

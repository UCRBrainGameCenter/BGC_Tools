using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Pads underlying stream
    /// </summary>
    public class StreamPadder : SimpleBGCFilter
    {
        public enum StimulusPlacement
        {
            Front = 0,
            Center,
            Back
        }

        public override int Channels => stream.Channels;

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        private readonly int prependedSamples;
        private readonly int appendedSamples;

        private readonly int appendStartSample;

        private readonly TransformRMSBehavior rmsBehavior;

        public int Position { get; private set; }

        public StreamPadder(
            IBGCStream stream,
            double totalDuration,
            StimulusPlacement stimPlacement,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException("Cannot pad infinite stream using stimPlacement");
            }

            int requestedSamples = (int)Math.Round(totalDuration * SamplingRate);
            int paddingSamples = requestedSamples - stream.ChannelSamples;

            this.rmsBehavior = rmsBehavior;

            if (paddingSamples < 0)
            {
                throw new StreamCompositionException("Stimulus exceeds targeted padding length");
            }

            switch (stimPlacement)
            {
                case StimulusPlacement.Front:
                    appendedSamples = 0;
                    prependedSamples = paddingSamples;
                    break;

                case StimulusPlacement.Center:
                    //Putting our extra sample in front
                    appendedSamples = paddingSamples / 2;
                    prependedSamples = paddingSamples - appendedSamples;
                    break;

                case StimulusPlacement.Back:
                    appendedSamples = paddingSamples;
                    prependedSamples = 0;
                    break;

                default:
                    throw new NotSupportedException($"Stimulus Placement not supported: {stimPlacement}");
            }

            appendStartSample = stream.ChannelSamples + prependedSamples;
            ChannelSamples = requestedSamples;
            TotalSamples = Channels * ChannelSamples;

            Reset();
        }

        public StreamPadder(
            IBGCStream stream,
            double prependDuration,
            double appendDuration,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (prependDuration < 0.0)
            {
                throw new StreamCompositionException("Cannot prepend negative durations");
            }

            if (appendDuration < 0.0)
            {
                throw new StreamCompositionException("Cannot append negative durations");
            }

            prependedSamples = (int)Math.Round(prependDuration * stream.SamplingRate);
            appendedSamples = (int)Math.Round(appendDuration * stream.SamplingRate);

            this.rmsBehavior = rmsBehavior;

            if (stream.ChannelSamples == int.MaxValue)
            {
                if (appendedSamples != 0)
                {
                    throw new StreamCompositionException("Cannot append padding to an infinite stream");
                }

                appendStartSample = int.MaxValue;
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                appendStartSample = stream.ChannelSamples + prependedSamples;
                ChannelSamples = stream.ChannelSamples + prependedSamples + appendedSamples;
                TotalSamples = stream.Channels * ChannelSamples;
            }

            Reset();
        }

        public StreamPadder(
            IBGCStream stream,
            int prependSamples,
            int appendSamples,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (prependSamples < 0)
            {
                throw new StreamCompositionException("Cannot prepend negative durations");
            }

            if (appendSamples < 0)
            {
                throw new StreamCompositionException("Cannot append negative durations");
            }

            this.rmsBehavior = rmsBehavior;

            if (stream.ChannelSamples == int.MaxValue)
            {
                if (appendedSamples != 0)
                {
                    throw new StreamCompositionException("Cannot append padding to an infinite stream");
                }

                appendStartSample = int.MaxValue;
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                appendStartSample = stream.ChannelSamples + prependedSamples;
                ChannelSamples = stream.ChannelSamples + prependedSamples + appendedSamples;
                TotalSamples = stream.Channels * ChannelSamples;
            }

            Reset();
        }

        public override void Reset()
        {
            Position = 0;
            stream.Reset();
        }

        public override void Seek(int position)
        {
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);
            stream.Seek(Math.Max(0, Position - prependedSamples));
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (ChannelSamples != int.MaxValue)
            {
                count = Math.Min(count, Channels * (ChannelSamples - Position));
            }

            int remainingSamples = count;

            while (remainingSamples > 0)
            {
                if (Position < prependedSamples)
                {
                    int copySamples = Math.Min(remainingSamples, Channels * (prependedSamples - Position));

                    Array.Clear(data, offset, copySamples);

                    Position += copySamples / Channels;
                    offset += copySamples;
                    remainingSamples -= copySamples;
                }
                else if (Position < appendStartSample)
                {
                    //Not worried about int overflow here - we're protected by the limits of absurdity
                    int samplesToCopy = Math.Min(remainingSamples, Channels * (appendStartSample - Position));

                    int copySamples = stream.Read(data, offset, samplesToCopy);

                    if (copySamples == 0)
                    {
                        Debug.LogWarning("Didn't finish reading expected samples and hit the end.");
                        break;
                    }

                    Position += copySamples / Channels;
                    offset += copySamples;
                    remainingSamples -= copySamples;
                }
                else
                {
                    //Not worried about int overflow here - we're protected by the limits of absurdity
                    int copySamples = Math.Min(remainingSamples, Channels * (ChannelSamples - Position));

                    if (copySamples == 0)
                    {
                        Debug.LogWarning("Didn't finish reading expected samples and hit the end.");
                        break;
                    }

                    Array.Clear(data, offset, copySamples);

                    Position += copySamples / Channels;
                    offset += copySamples;
                    remainingSamples -= copySamples;
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

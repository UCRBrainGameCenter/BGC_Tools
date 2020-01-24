using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Centers the underlying stream, temporally, in a window of specified duration, or appends 
    /// specified number of silent samples to the beginning and end of the stream.
    /// </summary>
    public class StreamCenterer : SimpleBGCFilter
    {
        private readonly int totalChannelSamples;
        private readonly int preDelaySamples;
        private readonly int postDelaySamples;
        private readonly int postDelayStart;

        private int position = 0;

        public override int Channels => stream.Channels;

        public override int TotalSamples => Channels * totalChannelSamples;

        public override int ChannelSamples => totalChannelSamples;

        public StreamCenterer(IBGCStream stream, double totalDuration)
            : base(stream)
        {
            if (stream.Duration() > totalDuration)
            {
                throw new StreamCompositionException("StreamCenterer cannot center a stream inside a window of greater duration.");
            }

            totalChannelSamples = (int)Math.Ceiling(totalDuration * SamplingRate);
            int delaySamples = totalChannelSamples - stream.ChannelSamples;

            postDelaySamples = delaySamples / 2;
            preDelaySamples = delaySamples - postDelaySamples;
            postDelayStart = preDelaySamples + stream.ChannelSamples;
        }

        public StreamCenterer(IBGCStream stream, int preDelaySamples, int postDelaySamples)
            : base(stream)
        {
            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException("StreamCenterer cannot operate on infinite streams.");
            }

            this.postDelaySamples = postDelaySamples;
            this.preDelaySamples = preDelaySamples;
            postDelayStart = preDelaySamples + stream.ChannelSamples;
            totalChannelSamples = postDelayStart + postDelaySamples;
        }

        public override int Read(float[] data, int offset, int count)
        {
            count = Math.Min(count, Channels * (totalChannelSamples - position));
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                if (position < preDelaySamples)
                {
                    int copySamples = Math.Min(samplesRemaining, Channels * (preDelaySamples - position));

                    Array.Clear(data, offset, copySamples);

                    position += copySamples / Channels;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
                else if (position < postDelayStart)
                {
                    int samplesToCopy = Math.Min(samplesRemaining, Channels * (postDelayStart - position));

                    int copySamples = stream.Read(data, offset, samplesToCopy);

                    if (copySamples == 0)
                    {
                        Debug.LogWarning("Didn't finish reading expected samples and hit the end.");
                        break;
                    }

                    position += copySamples / Channels;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
                else
                {
                    int copySamples = Math.Min(samplesRemaining, Channels * (totalChannelSamples - position));

                    if (copySamples == 0)
                    {
                        Debug.LogWarning("Didn't finish reading expected samples and hit the end.");
                        break;
                    }

                    Array.Clear(data, offset, copySamples);

                    position += copySamples / Channels;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
            }

            return count - samplesRemaining;
        }

        public override void Reset()
        {
            stream.Reset();
            position = 0;
        }

        public override void Seek(int position)
        {
            stream.Seek(position - preDelaySamples);
            this.position = GeneralMath.Clamp(position, 0, totalChannelSamples);
        }

        public override IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();
    }
}

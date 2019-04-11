using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Upchannels the stream, including only the indicated channels
    /// </summary>
    public class StreamSelectiveUpChanneler : SimpleBGCFilter
    {
        public override int TotalSamples => 2 * ChannelSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => 2;

        private readonly AudioChannel channels;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public StreamSelectiveUpChanneler(IBGCStream stream, AudioChannel channels)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new ArgumentException("StreamSelectiveUpChanneler inner stream but have only one channel.");
            }

            this.channels = channels;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                int samplesToRead = Math.Min(BUFFER_SIZE, samplesRemaining / 2);
                int samplesRead = stream.Read(buffer, 0, samplesToRead);

                if (samplesRead <= 0)
                {
                    //We ran out of samples.
                    break;
                }

                switch (channels)
                {
                    case AudioChannel.Left:
                    case AudioChannel.Right:
                        int channelAdj = (int)channels;
                        for (int i = 0; i < samplesRead; i++)
                        {
                            data[offset + 2 * i + channelAdj] = buffer[i];
                        }
                        break;

                    case AudioChannel.Both:
                        for (int i = 0; i < samplesRead; i++)
                        {
                            data[offset + 2 * i] = buffer[i];
                            data[offset + 2 * i + 1] = buffer[i];
                        }
                        break;

                    default:
                        Debug.LogError($"Unexpected AudioChannel: {channels}");
                        goto case AudioChannel.Both;
                }

                offset += 2 * samplesRead;
                samplesRemaining -= 2 * samplesRead;
            }

            return count - samplesRemaining;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double innerRMS = stream.GetChannelRMS().First();
                _channelRMS = Enumerable.Repeat(innerRMS, Channels).ToArray();
            }

            return _channelRMS;
        }
    }
}

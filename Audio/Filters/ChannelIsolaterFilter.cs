using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Extracts the channel with the specified index as an independent stream.
    /// </summary>
    public class ChannelIsolaterFilter : SimpleBGCFilter
    {
        public override int Channels => 1;

        public override int TotalSamples => stream.ChannelSamples;

        public override int ChannelSamples => stream.ChannelSamples;

        private readonly int channelIndex;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public ChannelIsolaterFilter(IBGCStream stream, int channelIndex)
            : base(stream)
        {
            this.channelIndex = channelIndex;

            if (channelIndex >= stream.Channels)
            {
                throw new StreamCompositionException(
                    "ChannelIsolaterFilter channel index out of range.");
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            do
            {
                int channels = stream.Channels;
                int copySamples = Math.Min(BUFFER_SIZE, channels * samplesRemaining);

                int readSamples = stream.Read(buffer, 0, copySamples);
                int readChannelSamples = readSamples / channels;

                for (int i = 0; i < readChannelSamples; i++)
                {
                    data[offset + i] = buffer[i * channels + channelIndex];
                }

                offset += readChannelSamples;
                samplesRemaining -= readChannelSamples;

                if (readSamples < copySamples)
                {
                    break;
                }

            }
            while (samplesRemaining > 0);

            return count - samplesRemaining;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = stream.GetChannelRMS().Skip(channelIndex).Take(1);
            }

            return _channelRMS;
        }
    }
}

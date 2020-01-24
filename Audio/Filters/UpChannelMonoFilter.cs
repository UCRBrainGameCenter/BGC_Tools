using System;
using System.Linq;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Upchannels an underlying stream
    /// </summary>
    public class UpChannelMonoFilter : SimpleBGCFilter
    {
        public override int TotalSamples { get; }
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels { get; }

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public UpChannelMonoFilter(IBGCStream stream, int channelCount = 2)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("UpChannelMonoFilter inner stream must have only one channel.");
            }

            Channels = channelCount;

            if (ChannelSamples == int.MaxValue)
            {
                TotalSamples = int.MaxValue;
            }
            else
            {
                TotalSamples = Channels * ChannelSamples;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                int samplesToRead = Math.Min(BUFFER_SIZE, samplesRemaining / Channels);
                int samplesRead = stream.Read(buffer, 0, samplesToRead);

                if (samplesRead <= 0)
                {
                    //We ran out of samples.
                    break;
                }

                for (int i = 0; i < samplesRead; i++)
                {
                    for (int chan = 0; chan < Channels; chan++)
                    {
                        data[offset + Channels * i + chan] = buffer[i];
                    }
                }

                offset += Channels * samplesRead;
                samplesRemaining -= Channels * samplesRead;
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

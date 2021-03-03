using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Takes multiple streams and applies each to a different channel in the output
    /// </summary>
    public class StreamMergeFilter : BGCFilter
    {
        private readonly List<IBGCStream> streams = new List<IBGCStream>();
        public override IEnumerable<IBGCStream> InternalStreams => streams;

        public override int Channels => streams.Count;

        private int _totalSampleCount = 0;
        public override int TotalSamples => _totalSampleCount;

        private int _channelSampleCount = 0;
        public override int ChannelSamples => _channelSampleCount;

        private float _samplingRate;
        public override float SamplingRate => _samplingRate;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public StreamMergeFilter()
        {
            UpdateStats();
        }

        public StreamMergeFilter(params IBGCStream[] streams)
        {
            AddStreams(streams);
        }

        public StreamMergeFilter(IEnumerable<IBGCStream> streams)
        {
            AddStreams(streams);
        }

        public void AddStream(IBGCStream stream)
        {
            streams.Add(stream);
            UpdateStats();
        }

        public void AddStreams(IEnumerable<IBGCStream> streams)
        {
            this.streams.AddRange(streams);
            UpdateStats();
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            do
            {
                int channelSamples = samplesRemaining / Channels;
                int copySamples = Math.Min(BUFFER_SIZE, channelSamples);
                int maxReadChannelSamples = 0;

                for (int i = 0; i < Channels; i++)
                {
                    int readChannelSamples = streams[i].Read(buffer, 0, copySamples);
                    maxReadChannelSamples = Math.Max(maxReadChannelSamples, readChannelSamples);

                    for (int j = 0; j < readChannelSamples; j++)
                    {
                        data[offset + Channels * j + i] = buffer[j];
                    }
                }

                offset += maxReadChannelSamples * Channels;
                samplesRemaining -= maxReadChannelSamples * Channels;

                if (maxReadChannelSamples < copySamples)
                {
                    break;
                }

            }
            while (samplesRemaining > 0);

            return count - samplesRemaining;
        }

        public override void Reset() => streams.ForEach(x => x.Reset());

        public override void Seek(int position) => streams.ForEach(x => x.Seek(position));

        private void UpdateStats()
        {
            if (streams.Count > 0)
            {
                if (streams.Select(x => x.Channels).Max() != 1)
                {
                    throw new StreamCompositionException("StreamMergeFilter requires all streams have one channel each.");
                }

                IEnumerable<float> samplingRates = streams.Select(x => x.SamplingRate);
                _samplingRate = samplingRates.Max();

                if (_samplingRate != samplingRates.Min())
                {
                    throw new StreamCompositionException("StreamMergeFilter requires all streams have the same samplingRate.");
                }

                IEnumerable<int> channelSampleCounts = streams.Select(x => x.ChannelSamples);
                _channelSampleCount = channelSampleCounts.Max();
                if (_channelSampleCount != channelSampleCounts.Min())
                {
                    throw new StreamCompositionException("StreamMergeFilter requires all streams have the same number of samples.");
                }

                if (_channelSampleCount == int.MaxValue)
                {
                    _totalSampleCount = int.MaxValue;
                }
                else
                {
                    _totalSampleCount = Channels * _channelSampleCount;
                }

                _channelRMS = null;
            }
            else
            {
                _channelSampleCount = 0;
                _totalSampleCount = 0;
                _samplingRate = 44100f;
                _channelRMS = null;
            }
        }

        public static StreamMergeFilter SafeMerge(params IBGCStream[] streams) =>
            SafeMerge(streams as IEnumerable<IBGCStream>);

        public static StreamMergeFilter SafeMerge(IEnumerable<IBGCStream> streams)
        {
            IEnumerable<int> channelSamples = streams.Select(x => x.ChannelSamples);
            int maxChannelSampleCount = channelSamples.Max();
            if (maxChannelSampleCount == channelSamples.Min())
            {
                return new StreamMergeFilter(streams);
            }

            IEnumerable<IBGCStream> inputStreams = streams.Select(x => new StreamCenterer(x, 0, maxChannelSampleCount - x.ChannelSamples));
            return new StreamMergeFilter(inputStreams);
        }


        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = streams.Select(x => x.GetChannelRMS().First());
            }

            return _channelRMS;
        }
    }
}

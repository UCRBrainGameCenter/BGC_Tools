using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Additive Synthesis - Output samples are equal to the linear sum of input samples.
    /// </summary>
    public class StreamAdder : BGCFilter
    {
        private readonly List<IBGCStream> streams = new List<IBGCStream>();
        public override IEnumerable<IBGCStream> InternalStreams => streams;

        private int _channels = 1;
        public override int Channels => _channels;

        private int _totalSampleCount = 0;
        public override int TotalSamples => _totalSampleCount;

        private int _channelSampleCount = 0;
        public override int ChannelSamples => _channelSampleCount;

        private float _samplingRate;
        public override float SamplingRate => _samplingRate;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public StreamAdder()
        {
            UpdateStats();
        }

        public StreamAdder(params IBGCStream[] streams)
        {
            AddStreams(streams);
        }

        public StreamAdder(IEnumerable<IBGCStream> streams)
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

        public bool RemoveStream(IBGCStream stream)
        {
            bool success = streams.Remove(stream);
            UpdateStats();
            return success;
        }

        public override void Reset() => streams.ForEach(x => x.Reset());

        public override int Read(float[] data, int offset, int count)
        {
            int minRemainingSamples = count;

            Array.Clear(data, offset, count);

            foreach (IBGCStream stream in streams)
            {
                int streamRemainingSamples = count;
                int streamOffset = offset;

                while (streamRemainingSamples > 0)
                {
                    int maxRead = Math.Min(BUFFER_SIZE, streamRemainingSamples);
                    int streamReadSamples = stream.Read(buffer, 0, maxRead);

                    if (streamReadSamples == 0)
                    {
                        //Done with this stream
                        break;
                    }

                    for (int i = 0; i < streamReadSamples; i++)
                    {
                        data[streamOffset + i] += buffer[i];
                    }

                    streamOffset += streamReadSamples;
                    streamRemainingSamples -= streamReadSamples;
                }

                minRemainingSamples = Math.Min(minRemainingSamples, streamRemainingSamples);
            }

            return count - minRemainingSamples;
        }

        public override void Seek(int position) => streams.ForEach(x => x.Seek(position));

        private void UpdateStats()
        {
            if (streams.Count > 0)
            {
                IEnumerable<int> channels = streams.Select(x => x.Channels);
                _channels = channels.Max();
                if (_channels != channels.Min())
                {
                    throw new StreamCompositionException($"StreamAdder Channel Count Mismatch.");
                }

                IEnumerable<float> samplingRates = streams.Select(x => x.SamplingRate);
                _samplingRate = samplingRates.Max();

                if (_samplingRate != samplingRates.Min())
                {
                    throw new StreamCompositionException("StreamAdder requires all streams have the same samplingRate.");
                }

                _channelSampleCount = streams.Select(x => x.ChannelSamples).Max();

                if (_channelSampleCount == int.MaxValue)
                {
                    _totalSampleCount = int.MaxValue;
                }
                else
                {
                    _totalSampleCount = _channels * _channelSampleCount;
                }

                _channelRMS = null;
            }
            else
            {
                _channels = 1;
                _channelSampleCount = 0;
                _totalSampleCount = 0;
                _samplingRate = 44100f;
                _channelRMS = null;
            }
        }

        private IEnumerable<double> _channelRMS = null;
        //RMS for each channel will be the sum of the constituent RMS's
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = streams
                    .Select(x => x.GetChannelRMS())
                    .Aggregate(
                        seed: new double[Channels] as IEnumerable<double>,
                        func: (total, next) => total.Zip(next, (prior, second) => prior + second * second),
                        resultSelector: total => total.Select(Math.Sqrt))
                    .ToArray();
            }

            return _channelRMS;
        }
    }
}

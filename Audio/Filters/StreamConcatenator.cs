using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Appends multiple streams, end-to-end.  Expects each stream to have the same number of
    /// channels and sampling rate.
    /// </summary>
    public class StreamConcatenator : BGCFilter
    {
        private readonly List<IBGCStream> streams = new List<IBGCStream>();
        public override IEnumerable<IBGCStream> InternalStreams => streams;

        private int _channels = 1;
        public override int Channels => _channels;

        public override int TotalSamples => Channels * ChannelSamples;

        private int _channelSampleCount = 0;
        public override int ChannelSamples => _channelSampleCount;

        private float _samplingRate;
        public override float SamplingRate => _samplingRate;

        public int CurrentClipIndex { get; private set; }
        public int Position { get; private set; }

        public StreamConcatenator()
        {
            UpdateStats();
        }

        public StreamConcatenator(params IBGCStream[] streams)
        {
            AddStreams(streams);
        }

        public StreamConcatenator(IEnumerable<IBGCStream> streams)
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

        public override void Reset()
        {
            CurrentClipIndex = 0;
            Position = 0;
            streams.ForEach(x => x.Reset());
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0)
            {
                if (CurrentClipIndex >= streams.Count)
                {
                    //Hit the end
                    break;
                }

                IBGCStream stream = streams[CurrentClipIndex];

                int readSamples = stream.Read(data, offset, remainingSamples);

                remainingSamples -= readSamples;
                offset += readSamples;
                Position += readSamples / Channels;

                if (readSamples <= 0)
                {
                    CurrentClipIndex++;

                    if (CurrentClipIndex < streams.Count)
                    {
                        //Reset on advancing allows a concatenator to hold multiple 
                        //copies of the same clip
                        streams[CurrentClipIndex].Reset();
                    }
                }
            }

            return count - remainingSamples;
        }

        public override void Seek(int position)
        {
            Position = position;
            CurrentClipIndex = streams.Count;

            for (int i = 0; i < streams.Count; i++)
            {
                IBGCStream clip = streams[i];
                if (position > 0)
                {
                    //Seek
                    if (position > clip.ChannelSamples)
                    {
                        clip.Seek(clip.ChannelSamples);
                        position -= clip.ChannelSamples;
                    }
                    else
                    {
                        clip.Seek(position);
                        position = 0;
                        CurrentClipIndex = i;
                    }
                }
                else
                {
                    clip.Reset();
                }
            }
        }

        private void UpdateStats()
        {
            if (streams.Count > 0)
            {
                IEnumerable<int> channels = streams.Select(x => x.Channels);
                _channels = channels.Max();

                if (_channels != channels.Min())
                {
                    throw new Exception("AudioClipConcatenator requires all streams have the same channel count.");
                }

                IEnumerable<float> samplingRates = streams.Select(x => x.SamplingRate);
                _samplingRate = samplingRates.Max();

                if (_samplingRate != samplingRates.Min())
                {
                    throw new Exception("AudioClipConcatenator requires all streams have the same samplingRate.");
                }

                _channelSampleCount = streams.Select(x => x.ChannelSamples).Sum();
                _channelRMS = null;
            }
            else
            {
                _channels = 1;
                _channelSampleCount = 0;
                _samplingRate = 44100f;
                _channelRMS = null;
            }
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double[] rms = new double[Channels];

                foreach (IBGCStream stream in streams)
                {
                    double[] streamRMS = stream.GetChannelRMS().ToArray();

                    for (int i = 0; i < Channels; i++)
                    {
                        rms[i] += (stream.ChannelSamples / (double)ChannelSamples) * streamRMS[i];
                    }
                }

                for (int i = 0; i < Channels; i++)
                {
                    rms[i] = Math.Sqrt(rms[i]);
                }

                _channelRMS = rms;
            }

            return _channelRMS;
        }
    }
}

using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Caches the underlying stream as samples are read
    /// </summary>
    public class StreamCacher : SimpleBGCFilter
    {
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => stream.Channels;

        private readonly float[] sampleCache;
        private int cachePosition = 0;
        private int position = 0;
        private const int FRAME_SIZE = 512;

        public StreamCacher(IBGCStream stream)
            : base(stream)
        {
            if (stream.TotalSamples == int.MaxValue)
            {
                throw new StreamCompositionException("Can't cache unlimited samples");
            }

            sampleCache = new float[stream.TotalSamples];
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                if (position < cachePosition)
                {
                    //Read from cache
                    int samplesToReadFromCache = Math.Min(cachePosition - position, samplesRemaining);

                    for (int i = 0; i < samplesToReadFromCache; i++)
                    {
                        data[offset + i] = sampleCache[position + i];
                    }

                    position += samplesToReadFromCache;
                    offset += samplesToReadFromCache;
                    samplesRemaining -= samplesToReadFromCache;

                }
                else
                {
                    //Read from stream
                    int samplesToReadToCache = Math.Min(FRAME_SIZE, samplesRemaining);
                    samplesToReadToCache = Math.Min(samplesToReadToCache, TotalSamples - cachePosition);
                    int readSamples = stream.Read(sampleCache, cachePosition, samplesToReadToCache);

                    if (readSamples == 0)
                    {
                        //We ran out of samples
                        break;
                    }

                    for (int i = 0; i < readSamples; i++)
                    {
                        data[offset + i] = sampleCache[position + i];
                    }

                    cachePosition += readSamples;
                    position += readSamples;
                    offset += readSamples;
                    samplesRemaining -= readSamples;
                }
            }

            return count - samplesRemaining;
        }

        public override void Reset()
        {
            position = 0;
        }

        public override void Seek(int position)
        {
            position *= Channels;
            if (position < cachePosition)
            {
                //Jump back
                this.position = position;
            }
            else
            {
                //Read forward
                int samplesRemaining = position - cachePosition;

                while (samplesRemaining > 0)
                {
                    int samplesToReadToCache = Math.Min(FRAME_SIZE, samplesRemaining);
                    int readSamples = stream.Read(sampleCache, cachePosition, samplesToReadToCache);

                    if (readSamples == 0)
                    {
                        //We ran out of samples
                        break;
                    }

                    cachePosition += readSamples;
                    this.position += readSamples;
                    samplesRemaining -= readSamples;
                }
            }
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = stream.GetChannelRMS();
            }

            return _channelRMS;
        }
    }
}

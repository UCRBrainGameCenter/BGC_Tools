using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Loops underling stream indefinitely.
    /// </summary>
    public class StreamRepeater : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;


        public StreamRepeater(IBGCStream stream)
            : base(stream)
        {
        }

        public override void Reset() => stream.Reset();

        public override void Seek(int position)
        {
            position %= stream.ChannelSamples;
            if (position < 0)
            {
                position += stream.ChannelSamples;
            }

            stream.Seek(position);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;
            int lastResetAt = -1;

            while (remainingSamples > 0)
            {
                int readSamples = stream.Read(data, offset, remainingSamples);

                if (readSamples == 0)
                {
                    //Protect against a stream not returning more samples after a reset.
                    //This shouldn't happen, but we don't want to hang
                    if (remainingSamples == lastResetAt)
                    {
                        break;
                    }

                    lastResetAt = remainingSamples;

                    stream.Reset();
                }

                remainingSamples -= readSamples;
                offset += readSamples;
            }

            return count - remainingSamples;
        }

        public override IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();
    }

}

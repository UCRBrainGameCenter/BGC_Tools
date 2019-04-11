using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// This slow filter scans the whole sample and scales it between +1 and -1 so there
    /// is no clipping but maximum volume.
    /// Primarily intended for saving audio to files.
    /// </summary>
    public class SlowRangeFitterFilter : SimpleBGCFilter
    {
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;
        public override int Channels => stream.Channels;

        private float scalar = 1f;

        public SlowRangeFitterFilter(IBGCStream stream)
            : base(stream)
        {
        }

        protected override void _Initialize()
        {
            stream.Reset();
            float max = 0f;
            const int BUFFER_SIZE = 512;
            float[] buffer = new float[BUFFER_SIZE];
            int samplesRead;
            do
            {
                samplesRead = stream.Read(buffer, 0, BUFFER_SIZE);

                for (int i = 0; i < samplesRead; i++)
                {
                    max = Math.Max(Math.Abs(buffer[i]), max);
                }
            }
            while (samplesRead > 0);

            if (max != 0f)
            {
                scalar = 1f / max;
            }

            stream.Reset();
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int readSamples = stream.Read(data, offset, count);

            for (int i = 0; i < readSamples; i++)
            {
                data[i + offset] *= scalar;
            }

            return readSamples;
        }

        public override IEnumerable<double> GetChannelRMS() => throw new NotSupportedException();
    }
}

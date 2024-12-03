using System;
using System.Linq;
using System.Collections.Generic;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace BGC.Audio.Filters
{
    //testtest
    /// <summary>
    /// Segment a stream from start to duration
    /// </summary>
    public class Segmentor : SimpleBGCFilter
    {
        public override int TotalSamples => stream.TotalSamples;
        public override int Channels => stream.Channels;
        public override int ChannelSamples => stream.ChannelSamples;

        private bool randomStart;
        private double start, duration;
        private int counter, startSample, endSample;

        public Segmentor(
            IBGCStream stream,
            bool randomStart,
            double start,
            double duration)
            : base(stream)
        {
            this.randomStart = randomStart;
            this.start = start;
            this.duration = duration;

            CalculateStartAndEnd();
        }

        protected override void _Initialize()
        {
            //not sure what to do here
        }

        public override void Reset()
        {
            counter = 0;
            stream.Reset();
            CalculateStartAndEnd();
        }

        private void CalculateStartAndEnd()
        {
            float startTime;

            if (randomStart)
            {
                Random random = new Random();
                startTime = (float)(random.NextDouble() * (stream.Duration() - (duration / 1000f)));
            }
            else
            {
                startTime = (float)(start / 1000f);
            }

            float endTime = startTime + (float)(duration / 1000f);

            startSample = (int)(startTime * stream.SamplingRate);
            endSample = (int)(endTime * stream.SamplingRate);
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int copied = stream.Read(data, offset, count);

            for (int i = 0; i < copied; i++)
            {
                if (counter < startSample || counter > endSample)
                {
                    data[i] = 0;
                }
                counter++;
            }

            return copied;
        }

        public override IEnumerable<double> GetChannelRMS()
        {
            return stream.GetChannelRMS();
        }
    }
}

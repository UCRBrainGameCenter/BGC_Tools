using System;
using System.Collections.Generic;

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
        private double easeDuration;
        private int counter, startSample, endSample, easeSample;

        public Segmentor(
            IBGCStream stream,
            bool randomStart,
            double start,
            double duration,
            double easeDuration)
            : base(stream)
        {
            this.randomStart = randomStart;
            this.start = start;
            this.duration = duration;
            this.easeDuration = easeDuration;

            CalculateStartAndEnd();
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

            float easeInTime = (float)(easeDuration / 1000f);
            easeSample = (int)(easeInTime * stream.SamplingRate);

            if (easeSample > (endSample - startSample) / 2)
            {
                UnityEngine.Debug.Log("divide sample");
                easeSample = (endSample - startSample) / 2;
            }
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
                else
                {
                    data[i] *= CalculateEase();
                }
                counter++;
            }

            return copied;
        }

        private float CalculateEase()
        {
            if (counter < startSample + easeSample)
            {
                return UnityEngine.Mathf.InverseLerp(startSample, startSample + easeSample, counter);
            }
            else if (counter >= endSample - easeSample)
            {
                return UnityEngine.Mathf.InverseLerp(endSample, endSample - easeSample, counter);
            }

            return 1;
        }

        public override IEnumerable<double> GetChannelRMS()
        {
            return stream.GetChannelRMS();
        }
    }
}

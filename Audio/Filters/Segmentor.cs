using System;
using System.Collections.Generic;
using UnityEngine;

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

        private int easeSampleStart, easeSampleEnd;
        private double easeDurationStart, easeDurationEnd;
        private double easeOffsetStart, easeOffsetEnd;
        private EasingType easeTypeStart, easeTypeEnd;

        private bool isMirrored;

        public Segmentor(
            IBGCStream stream,
            bool randomStart,
            double start,
            double duration,
            FixedEaseBehaviour startEaseBehaviour,
            IEaseBehaviour endEaseBehaviour
            )
            : base(stream)
        {
            this.randomStart = randomStart;
            this.start = start;
            this.duration = duration;

            easeDurationStart = startEaseBehaviour.EaseDuration;
            easeOffsetStart = startEaseBehaviour.EaseOffset;
            easeTypeStart = startEaseBehaviour.EaseType;

            UnityEngine.Debug.Log(endEaseBehaviour.GetType());
            if (endEaseBehaviour is FixedEaseBehaviour endEaseBehaviourFixed)
            {
                easeDurationEnd = endEaseBehaviourFixed.EaseDuration;
                easeOffsetEnd = endEaseBehaviourFixed.EaseOffset;
                easeTypeEnd = endEaseBehaviourFixed.EaseType;
                isMirrored = false;
            }
            else
            {
                easeDurationEnd = easeDurationStart;
                easeOffsetEnd = easeOffsetStart;
                easeTypeEnd = easeTypeStart;
                isMirrored = true;
            }

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

            float easeOffsetStartTime = (float)(easeOffsetStart / 1000f);
            float easeOffsetEndTime = (float)(easeOffsetEnd / 1000f);

            startTime -= easeOffsetStartTime;
            endTime += easeOffsetEndTime;

            startSample = (int)(startTime * stream.SamplingRate);
            endSample = (int)(endTime * stream.SamplingRate);

            float easeInTimeStart = (float)(easeDurationStart / 1000f);
            float easeInTimeEnd = (float)(easeDurationEnd / 1000f);

            easeSampleStart = (int)(easeInTimeStart * stream.SamplingRate);
            easeSampleEnd = (int)(easeInTimeEnd * stream.SamplingRate);
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
                    float ease = CalculateEase();
                    //UnityEngine.Debug.Log(ease);
                    data[i] *= ease;
                }
                counter++;
            }

            return copied;
        }

        private float CalculateEase()
        {
            bool inEaseStart = counter < startSample + easeSampleStart;
            bool inEaseEnd = counter >= endSample - easeSampleEnd;

            if (inEaseStart && inEaseEnd) //if in both, use smallest ease value
            {
                float percentStart = UnityEngine.Mathf.InverseLerp(startSample, startSample + easeSampleStart, counter);
                float percentEnd = UnityEngine.Mathf.InverseLerp(endSample, endSample - easeSampleEnd, counter);

                float easedStart = EasingFunctions.ApplyEasing(easeTypeStart, percentStart);
                float easedEnd = EasingFunctions.ApplyEasing(easeTypeEnd, percentEnd);

                return Mathf.Min(easedStart, easedEnd);
            }
            else if (inEaseStart)
            {
                float percent = UnityEngine.Mathf.InverseLerp(startSample, startSample + easeSampleStart, counter);
                return EasingFunctions.ApplyEasing(easeTypeStart, percent);
            }
            else if (inEaseEnd)
            {
                float percent = UnityEngine.Mathf.InverseLerp(endSample, endSample - easeSampleEnd, counter);
                return EasingFunctions.ApplyEasing(easeTypeEnd, percent);
            }

            return 1;
        }

        public override IEnumerable<double> GetChannelRMS()
        {
            return stream.GetChannelRMS();
        }
    }
}

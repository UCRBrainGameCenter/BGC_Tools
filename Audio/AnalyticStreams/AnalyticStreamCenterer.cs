using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// A Decorator class for AnalyticStreams that centers the stream, temporally, in a window.
    /// </summary>
    public class AnalyticStreamCenterer : SimpleAnalyticFilter
    {
        private readonly int totalSamples;
        private readonly int preDelaySamples;
        private readonly int postDelaySamples;
        private readonly int postDelayStart;

        private int position = 0;

        public override int Samples => totalSamples;

        public AnalyticStreamCenterer(IAnalyticStream stream, double totalDuration)
            : base(stream)
        {
            totalSamples = (int)Math.Ceiling(totalDuration * SamplingRate);
            int delaySamples = totalSamples - stream.Samples;

            postDelaySamples = delaySamples / 2;
            preDelaySamples = delaySamples - postDelaySamples;
            postDelayStart = preDelaySamples + stream.Samples;
        }

        public AnalyticStreamCenterer(IAnalyticStream stream, int preDelaySamples, int postDelaySamples)
            : base(stream)
        {
            this.postDelaySamples = postDelaySamples;
            this.preDelaySamples = preDelaySamples;
            postDelayStart = preDelaySamples + stream.Samples;
            totalSamples = postDelayStart + postDelaySamples;
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            count = Math.Min(count, totalSamples - position);
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                if (position < preDelaySamples)
                {
                    int copySamples = Math.Min(samplesRemaining, preDelaySamples - position);

                    Array.Clear(data, offset, copySamples);

                    position += copySamples;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
                else if (position < postDelayStart)
                {
                    int samplesToCopy = Math.Min(samplesRemaining, postDelayStart - position);

                    int copySamples = stream.Read(data, offset, samplesToCopy);

                    if (copySamples == 0)
                    {
                        Debug.LogWarning("Didn't finish reading expected samples and hit the end.");
                        position = postDelayStart;
                    }

                    position += copySamples;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
                else
                {
                    int copySamples = Math.Min(samplesRemaining, totalSamples - position);

                    Array.Clear(data, offset, copySamples);

                    position += copySamples;
                    offset += copySamples;
                    samplesRemaining -= copySamples;
                }
            }

            return count - samplesRemaining;
        }

        public override void Reset()
        {
            stream.Reset();
            position = 0;
        }

        public override void Seek(int position)
        {
            stream.Seek(position - preDelaySamples);
            this.position = GeneralMath.Clamp(position, 0, totalSamples);
        }

        public override double GetRMS() => stream.GetRMS();
    }

}

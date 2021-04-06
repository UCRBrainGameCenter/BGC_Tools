using BGC.Mathematics;
using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    public class CyclicalRotatorFilter : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        private readonly int sampleOffset;
        private int currentSample;

        private int ChannelSamplesRemaining => ChannelSamples - currentSample;

        public CyclicalRotatorFilter(
            IBGCStream stream,
            int sampleOffset)
            : base(stream)
        {
            this.sampleOffset = sampleOffset;

            if (sampleOffset >= stream.ChannelSamples)
            {
                throw new StreamCompositionException($"CyclicalRotatorFilter cannot use a sample offset greater than the " +
                    $"number of per-channel samples in the input stream. " +
                    $"Received {sampleOffset}, Stream samples-per-channel {stream.ChannelSamples}");
            }

            Reset();
        }

        public CyclicalRotatorFilter(
            IBGCStream stream,
            double timeOffset)
            : base(stream)
        {
            int sampleOffset = (int)Math.Round(timeOffset * SamplingRate);

            this.sampleOffset = sampleOffset;

            if (sampleOffset >= stream.ChannelSamples)
            {
                throw new StreamCompositionException($"CyclicalRotatorFilter cannot use a sample offset greater than the " +
                    $"number of per-channel samples in the input stream. " +
                    $"Received {sampleOffset}, Stream samples-per-channel {stream.ChannelSamples}");
            }

            Reset();
        }

        public override void Reset()
        {
            currentSample = 0;
            stream.Reset();
            stream.Seek(sampleOffset);
        }

        public override void Seek(int position)
        {
            currentSample = GeneralMath.Clamp(position, 0, ChannelSamples);

            if (position + sampleOffset >= ChannelSamples)
            {
                stream.Seek(ChannelSamples - (position + sampleOffset));
            }
            else
            {
                stream.Seek(position + sampleOffset);
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToRead = Math.Min(count, Channels * ChannelSamplesRemaining);

            int samplesRead = stream.Read(data, offset, samplesToRead);

            if (samplesRead < samplesToRead)
            {
                //More samples left

                if (currentSample + samplesRead / Channels + sampleOffset != ChannelSamples)
                {
                    UnityEngine.Debug.LogWarning($"CyclicalRotatorFilter reset early.  Possible filter error.");
                }

                //Reset stream and continue
                stream.Reset();

                samplesRead += stream.Read(data, offset + samplesRead,  samplesToRead - samplesRead);
            }

            currentSample += samplesRead / Channels;

            return samplesRead;
        }

        public override IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();
    }
}

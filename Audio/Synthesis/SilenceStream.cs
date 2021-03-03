using System;
using System.Collections.Generic;
using BGC.Mathematics;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Stream representing silence.
    /// Useful for concatenation or as a building-block for some stimuli.
    /// </summary>
    public class SilenceStream : BGCStream, IBGCEnvelopeStream, IBGCStream
    {
        public override int Channels { get; }
        public override float SamplingRate => 44100f;

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        private int position = 0;

        public SilenceStream(int channels, int channelSamples)
        {
            Channels = channels;
            ChannelSamples = channelSamples;

            if (ChannelSamples == int.MaxValue)
            {
                TotalSamples = int.MaxValue;
            }
            else
            {
                TotalSamples = Channels * ChannelSamples;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int readCount = Math.Min(Channels * (ChannelSamples - position), count);
            Array.Clear(data, offset, readCount);

            position += readCount / Channels;
            return readCount;
        }

        public override void Reset() => position = 0;

        public override void Seek(int position) =>
            this.position = GeneralMath.Clamp(position, 0, ChannelSamples);

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return 0.0;
        }

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => ChannelSamples;
        bool IBGCEnvelopeStream.HasMoreSamples() => position < ChannelSamples;
        float IBGCEnvelopeStream.ReadNextSample() => 0f;

        #endregion IBGCEnvelopeStream
    }
}

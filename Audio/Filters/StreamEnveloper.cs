using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Applies the specified envelope to the underlying stream.
    /// </summary>
    public class StreamEnveloper : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        private readonly IBGCEnvelopeStream envelopeStream;

        private readonly TransformRMSBehavior rmsBehavior;
        private int position = 0;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public StreamEnveloper(
            IBGCStream stream,
            IBGCEnvelopeStream envelopeStream,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            this.rmsBehavior = rmsBehavior;
            this.envelopeStream = envelopeStream;

            ChannelSamples = Math.Min(envelopeStream.Samples, stream.ChannelSamples);

            if (ChannelSamples == int.MaxValue)
            {
                TotalSamples = int.MaxValue;
            }
            else
            {
                TotalSamples = Channels * ChannelSamples;
            }
        }

        public override void Reset()
        {
            position = 0;
            stream.Reset();
            envelopeStream.Reset();
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, ChannelSamples);

            this.position = position;
            stream.Seek(position);
            envelopeStream.Seek(position);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToRead = Math.Min(
                ChannelSamples - position,
                count / Channels);

            int readSamples = stream.Read(data, offset, Channels * samplesToRead);

            if (readSamples == 0)
            {
                //No more stream samples
                return 0;
            }

            int envelopeSamplesRemaining = readSamples / Channels;

            while (envelopeSamplesRemaining > 0)
            {
                int envelopeSamplesToRead = Math.Min(BUFFER_SIZE, envelopeSamplesRemaining);

                int envelopeSamplesRead = envelopeStream.Read(buffer, 0, envelopeSamplesToRead);

                if (envelopeSamplesRead == 0)
                {
                    //No more envelope samples
                    return readSamples - Channels * envelopeSamplesRemaining;
                }

                for (int i = 0; i < envelopeSamplesRead; i++)
                {
                    for (int chan = 0; chan < Channels; chan++)
                    {
                        data[offset + Channels * i + chan] *= buffer[i];
                    }
                }

                envelopeSamplesRemaining -= envelopeSamplesRead;
                offset += Channels * envelopeSamplesRead;
                position += envelopeSamplesRead;
            }


            return readSamples;
        }

        protected override void _Initialize()
        {
            envelopeStream.Initialize();
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        _channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        _channelRMS = stream.GetChannelRMS();

                        if (_channelRMS.Any(double.IsNaN) && ChannelSamples != int.MaxValue)
                        {
                            goto case TransformRMSBehavior.Recalculate;
                        }
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return _channelRMS;
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Scales the underlying stream to a fixed RMS
    /// </summary>
    public class StreamRMSStandardizer : SimpleBGCFilter
    {
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => 1;

        private readonly double rms;
        private float scalar = 1f;

        public StreamRMSStandardizer(IBGCStream stream, double rms = (1.0 / 128.0))
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("StreamRMSStandardizer inner stream must have only one channel.");
            }

            this.rms = rms;
        }

        protected override void _Initialize()
        {
            double channelRMS = stream.GetChannelRMS().First();

            if (double.IsNaN(channelRMS))
            {
                if (stream.ChannelSamples == int.MaxValue)
                {
                    throw new StreamCompositionException("Unable to scale inifinte stream with unknowable RMS.");
                }

                channelRMS = stream.CalculateRMS().First();
            }

            scalar = (float)(rms / channelRMS);

            //Protect against some NaN Poisoning
            if (float.IsNaN(scalar) || float.IsInfinity(scalar))
            {
                scalar = 1f;
            }
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

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = new double[1] { rms };
            }

            return _channelRMS;
        }
    }
}

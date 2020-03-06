using System;
using System.Linq;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Clips the underlying stream between +1 and -1
    /// </summary>
    public class HardClipFilter : SimpleBGCFilter
    {
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => stream.Channels;

        public HardClipFilter(IBGCStream stream)
            : base(stream)
        {

        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRead = stream.Read(data, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                if (data[offset + i] > 1f || data[offset + i] < -1f)
                {
                    data[offset + i] = GeneralMath.Clamp(data[offset + i], -1f, 1f);
                }
            }

            return samplesRead;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                _channelRMS = this.CalculateRMS();
            }

            return _channelRMS;
        }
    }
}

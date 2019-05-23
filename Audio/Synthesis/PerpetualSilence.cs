using System;
using System.Collections.Generic;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Stream representing silence.
    /// Useful for concatenation or as a building-block for some stimuli.
    /// </summary>
    public class PerpetualSilence : BGCStream, IBGCEnvelopeStream, IBGCStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        public PerpetualSilence() { }

        public override int Read(float[] data, int offset, int count)
        {
            Array.Clear(data, offset, count);
            return count;
        }

        public override void Reset() { }

        public override void Seek(int position) { }

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return 0.0;
        }

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => int.MaxValue;
        bool IBGCEnvelopeStream.HasMoreSamples() => true;
        float IBGCEnvelopeStream.ReadNextSample() => 0f;

        #endregion IBGCEnvelopeStream
    }
}

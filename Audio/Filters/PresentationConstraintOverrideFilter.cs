using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Overrides the PresentationConstraints (for calibration purposes)
    /// </summary>
    public class PresentationConstraintOverrideFilter : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        private readonly IEnumerable<PresentationConstraints> presentationConstraints;

        public PresentationConstraintOverrideFilter(
            IBGCStream stream,
            IEnumerable<PresentationConstraints> presentationConstraints)
            : base(stream)
        {
            if (presentationConstraints.Count() != stream.Channels)
            {
                throw new StreamCompositionException($"Cannot override stream PresentationConstraints with mismatched channel count.");
            }

            this.presentationConstraints = presentationConstraints;
        }

        public PresentationConstraintOverrideFilter(
            IBGCStream stream,
            Audiometry.AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency)
            : base(stream)
        {
            PresentationConstraints presentationConstraints = new PresentationConstraints(
                calibrationSet: calibrationSet,
                frequency: frequency,
                compromise: false);

            this.presentationConstraints = Enumerable.Repeat(presentationConstraints, stream.Channels).ToArray();
        }

        public override int Read(float[] data, int offset, int count) => stream.Read(data, offset, count);

        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;
        public override IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();
    }
}

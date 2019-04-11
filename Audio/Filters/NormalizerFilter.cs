using System;
using System.Linq;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Scales an underlying stream to the desired level (dB SPL)
    /// </summary>
    public class NormalizerFilter : SimpleBGCFilter
    {
        public override int TotalSamples => 2 * stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => 2;

        private bool factorsInitialized = false;
        private float leftFactor;
        private float rightFactor;
        private readonly double presentationLevel;

        private readonly Calibration.Source source;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public NormalizerFilter(IBGCStream stream, double leftFactor, double rightFactor)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new ArgumentException("NormalizerFilter inner stream but have two channels.");
            }

            this.leftFactor = (float)leftFactor;
            this.rightFactor = (float)rightFactor;
            presentationLevel = 0.0;
            factorsInitialized = true;

            //Doesn't matter, we won't use this.
            source = Calibration.Source.MAX;
        }

        public NormalizerFilter(IBGCStream stream, double presentationLevel)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new ArgumentException("NormalizerFilter inner stream but have two channels.");
            }

            this.presentationLevel = presentationLevel;
            factorsInitialized = false;

            source = Calibration.Source.Custom;
        }

        public NormalizerFilter(
            IBGCStream stream,
            double presentationLevel,
            Calibration.Source source)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new ArgumentException("NormalizerFilter inner stream but have two channels.");
            }

            this.presentationLevel = presentationLevel;
            factorsInitialized = false;

            this.source = source;
        }

        protected override void _Initialize()
        {
            if (!factorsInitialized)
            {
                factorsInitialized = true;

                Normalization.GetRMSScalingFactors(
                    stream: stream,
                    desiredLevel: presentationLevel,
                    scalingFactorL: out double tempLeftFactor,
                    scalingFactorR: out double tempRightFactor,
                    source: source);

                leftFactor = (float)tempLeftFactor;
                rightFactor = (float)tempRightFactor;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                int samplesToRead = Math.Min(BUFFER_SIZE, samplesRemaining);
                int samplesRead = stream.Read(buffer, 0, samplesToRead);

                if (samplesRead == 0)
                {
                    //We ran out of samples.
                    break;
                }

                for (int i = 0; i < samplesRead; i += 2)
                {
                    data[offset + i] = leftFactor * buffer[i];
                    data[offset + i + 1] = rightFactor * buffer[i + 1];
                }

                offset += samplesRead;
                samplesRemaining -= samplesRead;
            }

            return count - samplesRemaining;
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double innerRMS = stream.GetChannelRMS().First();

                _channelRMS = new double[2] { leftFactor * innerRMS, rightFactor * innerRMS };
            }

            return _channelRMS;
        }
    }
}

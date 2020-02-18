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
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => 2;

        private bool factorsInitialized = false;
        private float leftFactor;
        private float rightFactor;
        private readonly (double levelL, double levelR) presentationLevels;

        private readonly Calibration.Source source;
        private readonly bool safetyLimit;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public NormalizerFilter(
            IBGCStream stream,
            double leftFactor,
            double rightFactor)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new StreamCompositionException("NormalizerFilter inner stream must have two channels.");
            }

            this.leftFactor = (float)leftFactor;
            this.rightFactor = (float)rightFactor;
            presentationLevels = (0.0, 0.0);
            factorsInitialized = true;

            //Doesn't matter, we bypass the normalizer.
            safetyLimit = false;
            source = Calibration.Source.MAX;
        }

        public NormalizerFilter(
            IBGCStream stream,
            double presentationLevel,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new StreamCompositionException("NormalizerFilter inner stream must have two channels.");
            }

            presentationLevels = (presentationLevel, presentationLevel);
            factorsInitialized = false;

            source = Calibration.Source.Custom;
            this.safetyLimit = safetyLimit;
        }

        public NormalizerFilter(
            IBGCStream stream,
            (double levelL, double levelR) presentationLevel,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new StreamCompositionException("NormalizerFilter inner stream must have two channels.");
            }

            presentationLevels = presentationLevel;
            factorsInitialized = false;

            source = Calibration.Source.Custom;
            this.safetyLimit = safetyLimit;
        }

        public NormalizerFilter(
            IBGCStream stream,
            double presentationLevel,
            Calibration.Source source,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new StreamCompositionException("NormalizerFilter inner stream must have two channels.");
            }

            presentationLevels = (presentationLevel, presentationLevel);
            factorsInitialized = false;

            this.source = source;
            this.safetyLimit = safetyLimit;
        }

        protected override void _Initialize()
        {
            if (!factorsInitialized)
            {
                factorsInitialized = true;

                Normalization.GetRMSScalingFactors(
                    stream: stream,
                    desiredLevel: presentationLevels.levelL,
                    scalingFactorL: out double tempLeftFactor,
                    scalingFactorR: out double tempRightFactor,
                    source: source,
                    safetyLimit: safetyLimit);

                if (presentationLevels.levelL != presentationLevels.levelR)
                {
                    Normalization.GetRMSScalingFactors(
                        stream: stream,
                        desiredLevel: presentationLevels.levelR,
                        scalingFactorL: out double _,
                        scalingFactorR: out tempRightFactor,
                        source: source,
                        safetyLimit: safetyLimit);
                }

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
                double[] innerRMS = stream.GetChannelRMS().ToArray();
                innerRMS[0] *= Math.Abs(leftFactor);
                innerRMS[1] *= Math.Abs(rightFactor);

                _channelRMS = innerRMS;
            }

            return _channelRMS;
        }
    }
}

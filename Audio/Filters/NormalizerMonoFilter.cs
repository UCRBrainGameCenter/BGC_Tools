using System;
using System.Linq;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Upchannels and scales an underlying single-channel stream to the specified level (dB SPL)
    /// </summary>
    public class NormalizerMonoFilter : SimpleBGCFilter
    {
        public override int TotalSamples => stream.ChannelSamples == int.MaxValue ? int.MaxValue : 2 * stream.ChannelSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override int Channels => 2;

        private bool factorsInitialized = false;
        private float leftFactor;
        private float rightFactor;
        private readonly (double levelL, double levelR) presentationLevels;
        private readonly bool safetyLimit;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public NormalizerMonoFilter(
            IBGCStream stream,
            double leftFactor,
            double rightFactor)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("NormalizerMonoFilter inner stream must have only one channel.");
            }

            this.leftFactor = (float)leftFactor;
            this.rightFactor = (float)rightFactor;
            presentationLevels = (0, 0);
            factorsInitialized = true;

            //Normalizer is bypassed
            safetyLimit = false;
        }

        public NormalizerMonoFilter(
            IBGCStream stream,
            double presentationLevel,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("NormalizerMonoFilter inner stream must have only one channel.");
            }

            presentationLevels = (presentationLevel, presentationLevel);
            factorsInitialized = false;
            this.safetyLimit = safetyLimit;
        }

        public NormalizerMonoFilter(
            IBGCStream stream,
            (double levelL, double levelR) levels,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("NormalizerMonoFilter inner stream must have only one channel.");
            }

            presentationLevels = levels;
            factorsInitialized = false;
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
                    safetyLimit: safetyLimit);

                if (presentationLevels.levelL != presentationLevels.levelR)
                {
                    Normalization.GetRMSScalingFactors(
                        stream: stream,
                        desiredLevel: presentationLevels.levelR,
                        scalingFactorL: out double _,
                        scalingFactorR: out tempRightFactor,
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
                int samplesToRead = Math.Min(BUFFER_SIZE, samplesRemaining / 2);
                int samplesRead = stream.Read(buffer, 0, samplesToRead);

                if (samplesRead == 0)
                {
                    //We ran out of samples.
                    break;
                }

                for (int i = 0; i < samplesRead; i++)
                {
                    data[offset + 2 * i] = leftFactor * buffer[i];
                    data[offset + 2 * i + 1] = rightFactor * buffer[i];
                }

                offset += 2 * samplesRead;
                samplesRemaining -= 2 * samplesRead;
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

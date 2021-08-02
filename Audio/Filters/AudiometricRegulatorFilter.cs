using System;
using System.Linq;
using System.Collections.Generic;
using BGC.Audio.Audiometry;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Scales an underlying stream to the desired level (dB HL)
    /// </summary>
    public class AudiometricRegulatorFilter : SimpleBGCFilter
    {
        public override int Channels => 2;
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        private bool factorsInitialized = false;
        private float leftFactor;
        private float rightFactor;

        private readonly double presentationLevelHL;
        private readonly AudiometricCalibration.CalibrationSet calibrationSet;
        private readonly double calibrationFrequency;
        private readonly AudiometricCalibration.Source source;
        private readonly bool safetyLimit;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public AudiometricRegulatorFilter(
            IBGCStream stream,
            double presentationLevelHL,
            AudiometricCalibration.CalibrationSet calibrationSet,
            double calibrationFrequency,
            bool safetyLimit = true)
            : base(stream)
        {
            if (stream.Channels != 2)
            {
                throw new StreamCompositionException("AudiometricRegulatorFilter inner stream must have two channels.");
            }

            this.presentationLevelHL = presentationLevelHL;
            this.calibrationSet = calibrationSet;
            this.calibrationFrequency = calibrationFrequency;

            this.safetyLimit = safetyLimit;
            source = AudiometricCalibration.Source.Custom;
            factorsInitialized = false;
        }

        protected override void _Initialize()
        {
            if (!factorsInitialized)
            {
                factorsInitialized = true;

                LevelRegulation.GetRMSScalingFactors(
                    stream: stream,
                    presentationLevelHL: presentationLevelHL,
                    calibrationSet: calibrationSet,
                    calibrationFrequency: calibrationFrequency,
                    scalingFactorL: out double tempLeftFactor,
                    scalingFactorR: out double tempRightFactor,
                    source: source,
                    safetyLimit: safetyLimit);

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
                if (!initialized)
                {
                    Initialize();
                }

                double[] innerRMS = stream.GetChannelRMS().ToArray();
                innerRMS[0] *= Math.Abs(leftFactor);
                innerRMS[1] *= Math.Abs(rightFactor);

                _channelRMS = innerRMS;
            }

            return _channelRMS;
        }
    }
}

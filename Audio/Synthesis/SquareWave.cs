using System;
using System.Collections.Generic;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Square wave stream. 
    /// </summary>
    public class SquareWave : BGCStream, IBGCEnvelopeStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly double frequency;
        private readonly float firstAmplitude;
        private readonly float secondAmplitude;
        private readonly double dutyCycle;

        private readonly double periodSamples;
        private readonly double upSamples;

        private double position = 0.0;

        public SquareWave(
            double amplitude,
            double frequency,
            double dutyCycle = 0.5)
        {
            firstAmplitude = (float)amplitude;
            secondAmplitude = (float)-amplitude;
            this.frequency = frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / this.frequency;
            upSamples = this.dutyCycle * periodSamples;
        }

        public SquareWave(
            double firstAmplitude,
            double secondAmplitude,
            double frequency,
            double dutyCycle = 0.5)
        {
            this.firstAmplitude = (float)firstAmplitude;
            this.secondAmplitude = (float)secondAmplitude;
            this.frequency = frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / this.frequency;
            upSamples = this.dutyCycle * periodSamples;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToRead = count;

            while (samplesToRead > 0)
            {
                int readingSamples;
                if (position < upSamples)
                {
                    //High
                    readingSamples = Math.Min(samplesToRead, (int)Math.Ceiling(upSamples - position));

                    for (int i = 0; i < readingSamples; i++)
                    {
                        data[offset + i] = firstAmplitude;
                    }
                }
                else
                {
                    //Low
                    readingSamples = Math.Min(samplesToRead, (int)Math.Ceiling(periodSamples - position));

                    for (int i = 0; i < readingSamples; i++)
                    {
                        data[offset + i] = secondAmplitude;
                    }

                }

                samplesToRead -= readingSamples;
                offset += readingSamples;
                position += readingSamples;

                if (position >= periodSamples)
                {
                    position -= periodSamples;
                }
            }

            return count;
        }

        public override void Reset()
        {
            position = 0.0;
        }

        public override void Seek(int position)
        {
            this.position = position;
            this.position %= periodSamples;
            if (this.position < 0f)
            {
                this.position += periodSamples;
            }
        }

        private double GetRMS() => Math.Sqrt(
            firstAmplitude * firstAmplitude * dutyCycle +
            secondAmplitude * secondAmplitude * (1 - dutyCycle));

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() => 
            _channelRMS ?? (_channelRMS = new double[] { GetRMS() });

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => int.MaxValue;

        bool IBGCEnvelopeStream.HasMoreSamples() => true;

        float IBGCEnvelopeStream.ReadNextSample()
        {
            float value = position++ < upSamples ? firstAmplitude : secondAmplitude;

            if (position >= periodSamples)
            {
                position -= periodSamples;
            }

            return value;
        }

        #endregion IBGCEnvelopeStream
    }
}

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

        private readonly double initialPosition;

        private double position = 0.0;

        public SquareWave(
            double amplitude,
            double frequency,
            double phase = 0.0,
            double dutyCycle = 0.5)
        {
            firstAmplitude = (float)amplitude;
            secondAmplitude = (float)-amplitude;
            this.frequency = frequency;
            this.dutyCycle = dutyCycle;


            periodSamples = SamplingRate / this.frequency;
            upSamples = this.dutyCycle * periodSamples;

            phase %= 2.0 * Math.PI;
            if (phase < 0.0)
            {
                phase += 2.0 * Math.PI;
            }

            initialPosition = phase * periodSamples / (2.0 * Math.PI);
            position = initialPosition;
        }

        public SquareWave(
            double firstAmplitude,
            double secondAmplitude,
            double frequency,
            double phase = 0.0,
            double dutyCycle = 0.5)
        {
            this.firstAmplitude = (float)firstAmplitude;
            this.secondAmplitude = (float)secondAmplitude;
            this.frequency = frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / this.frequency;
            upSamples = this.dutyCycle * periodSamples;

            phase %= 2.0 * Math.PI;
            if (phase < 0.0)
            {
                phase += 2.0 * Math.PI;
            }

            initialPosition = phase * periodSamples / (2.0 * Math.PI);
            position = initialPosition;
        }

        public SquareWave(
            double firstAmplitude,
            double secondAmplitude,
            ComplexCarrierTone carrier,
            double dutyCycle = 0.5)
        {
            this.firstAmplitude = (float)(firstAmplitude * carrier.amplitude.Magnitude);
            this.secondAmplitude = (float)(secondAmplitude * carrier.amplitude.Magnitude);
            frequency = carrier.frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / frequency;
            upSamples = this.dutyCycle * periodSamples;

            double phase = carrier.amplitude.Phase / (2.0 * Math.PI);
            phase %= 1.0;
            if (phase < 0.0)
            {
                phase += 1.0;
            }

            initialPosition = phase * periodSamples;
            position = initialPosition;
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
            position = initialPosition;
        }

        public override void Seek(int position)
        {
            this.position = initialPosition + position;
            this.position %= periodSamples;
            if (this.position < 0f)
            {
                this.position += periodSamples;
            }
        }

        private double GetRMS() => Math.Sqrt(
            firstAmplitude * firstAmplitude * dutyCycle +
            secondAmplitude * secondAmplitude * (1 - dutyCycle));

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() => 
            channelRMS ?? (channelRMS = new double[] { GetRMS() });

        private readonly IEnumerable<PresentationConstraints> presentationConstraints = new PresentationConstraints[1] { null };
        public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => presentationConstraints;

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

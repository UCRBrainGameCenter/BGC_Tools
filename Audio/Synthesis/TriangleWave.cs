using System;
using System.Collections.Generic;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Triangle wave stream with specified amplitude, frequency, and duty-cycle.
    /// </summary>
    public class TriangleWave : BGCStream, IBGCEnvelopeStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly double frequency;
        private readonly double amplitude;
        private readonly double dutyCycle;

        private readonly double periodSamples;
        private readonly double upSamples;
        private readonly double downSamples;

        private readonly double initialPosition;

        private double position = 0.0;

        public TriangleWave(
            double amplitude,
            double frequency,
            double phase = 0.0,
            double dutyCycle = 0.5)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / this.frequency;
            upSamples = this.dutyCycle * periodSamples;
            downSamples = periodSamples - upSamples;

            phase %= 2.0 * Math.PI;
            if (phase < 0.0)
            {
                phase += 2.0 * Math.PI;
            }

            initialPosition = phase * periodSamples / (2.0 * Math.PI);
            position = initialPosition;
        }

        public TriangleWave(
            ComplexCarrierTone carrierTone,
            double dutyCycle = 0.5)
        {
            amplitude = carrierTone.amplitude.Magnitude;
            frequency = carrierTone.frequency;
            this.dutyCycle = dutyCycle;

            periodSamples = SamplingRate / frequency;
            upSamples = this.dutyCycle * periodSamples;
            downSamples = periodSamples - upSamples;

            double phase = carrierTone.amplitude.Phase / (2.0 * Math.PI);
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
                    //Up
                    readingSamples = Math.Min(samplesToRead, (int)Math.Ceiling(upSamples - position));

                    double factor = 2 * amplitude / upSamples;
                    double diff = factor * position - amplitude;

                    for (int i = 0; i < readingSamples; i++)
                    {
                        data[offset + i] = (float)(factor * i + diff);
                    }
                }
                else
                {
                    //Down
                    readingSamples = Math.Min(samplesToRead, (int)Math.Ceiling(periodSamples - position));

                    double factor = -2 * amplitude / downSamples;
                    double diff = amplitude + factor * (position - upSamples);

                    for (int i = 0; i < readingSamples; i++)
                    {
                        data[offset + i] = (float)(factor * i + diff);
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

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() => 
            _channelRMS ?? (_channelRMS = new double[] { amplitude / Math.Sqrt(3) });

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => int.MaxValue;

        bool IBGCEnvelopeStream.HasMoreSamples() => true;

        float IBGCEnvelopeStream.ReadNextSample()
        {
            float value;

            if (position < upSamples)
            {
                //Up
                value = (float)(2 * amplitude * position++ / upSamples - amplitude);
            }
            else
            {
                //Down
                value = (float)(amplitude - 2 * amplitude * (position++ - upSamples) / downSamples);

            }

            if (position >= periodSamples)
            {
                position -= periodSamples;
            }

            return value;
        }

        #endregion IBGCEnvelopeStream
    }
}

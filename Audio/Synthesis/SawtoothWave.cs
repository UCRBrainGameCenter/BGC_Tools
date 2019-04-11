using System;
using System.Collections.Generic;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Sawtooth stream with specified frequency and amplitude
    /// </summary>
    public class SawtoothWave : BGCStream, IBGCEnvelopeStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly double frequency;
        private readonly double amplitude;

        private readonly double periodSamples;

        private double position = 0.0;

        public SawtoothWave(
            double amplitude,
            double frequency)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;

            periodSamples = SamplingRate / this.frequency;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesToRead = count;

            while (samplesToRead > 0)
            {
                int readingSamples = Math.Min(samplesToRead, (int)Math.Ceiling(periodSamples - position));

                double factor = 2 * amplitude / periodSamples;
                double diff = factor * position - amplitude;

                for (int i = 0; i < readingSamples; i++)
                {
                    data[offset + i] = (float)(factor * i + diff);
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

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() =>
            _channelRMS ?? (_channelRMS = new double[] { amplitude / Math.Sqrt(3) });

        #region IBGCEnvelopeStream

        int IBGCEnvelopeStream.Samples => int.MaxValue;

        bool IBGCEnvelopeStream.HasMoreSamples() => true;

        float IBGCEnvelopeStream.ReadNextSample()
        {
            float value = (float)(2 * amplitude * position++ / periodSamples - amplitude);

            if (position >= periodSamples)
            {
                position -= periodSamples;
            }

            return value;
        }

        #endregion IBGCEnvelopeStream
    }
}

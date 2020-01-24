using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// This implementation is based on a naive adaptation of the BiQuad filter in this pacakge,
    /// which is based on the one found in CSCore, which in turn was 
    /// based on http://www.earlevel.com/main/2011/01/02/biquad-formulas/
    /// </summary>
    public class ContinuousFilter : SimpleBGCFilter
    {
        public enum FilterType
        {
            HighPass = 0,
            LowPass,
            BandPass,
            MAX
        }

        private readonly FilterType filterType;
        private readonly double Q;

        private readonly double freqLB;
        private readonly double freqUB;
        private readonly double freqMid;
        private readonly double freqFactor;

        private double a0;
        private double a1;
        private double a2;
        private double b1;
        private double b2;

        private double Z1;
        private double Z2;

        private readonly TransformRMSBehavior rmsBehavior;

        private float lastFilterSample = float.NaN;

        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        public override int ChannelSamples => Math.Min(stream.ChannelSamples, filterEnvelope.Samples);

        private readonly IBGCEnvelopeStream filterEnvelope;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        _channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        _channelRMS = stream.GetChannelRMS();

                        if (_channelRMS.Any(double.IsNaN) && ChannelSamples != int.MaxValue)
                        {
                            goto case TransformRMSBehavior.Recalculate;
                        }
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return _channelRMS;
        }

        public ContinuousFilter(
            IBGCStream stream,
            IBGCEnvelopeStream filterEnvelope,
            FilterType filterType,
            double freqLB,
            double freqUB,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"ContinuousFilter expects a mono stream. " +
                    $"Received a stream with {stream.Channels} channels,");
            }

            this.filterType = filterType;

            if (double.IsNaN(qFactor))
            {
                Q = Math.Sqrt(0.5);
            }
            else
            {
                Q = qFactor;
            }

            this.filterEnvelope = filterEnvelope;

            this.freqLB = freqLB;
            this.freqUB = freqUB;
            freqMid = Math.PI * (this.freqUB + this.freqLB) / (2.0 * stream.SamplingRate);
            freqFactor = Math.PI * (this.freqUB - this.freqLB) / (2.0 * stream.SamplingRate);

            this.rmsBehavior = rmsBehavior;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                int samplesToRead = Math.Min(samplesRemaining, BUFFER_SIZE);

                int samplesRead = stream.Read(data, offset, samplesToRead);

                if (samplesRead <= 0)
                {
                    break;
                }

                int filterSamplesRead = filterEnvelope.Read(buffer, 0, samplesRead);

                if (filterSamplesRead <= 0)
                {
                    break;
                }

                for (int i = 0; i < filterSamplesRead; i++)
                {
                    UpdateCoefficients(buffer[i]);
                    data[offset + i] = ProcessSample(data[offset + i]);
                }

                samplesRemaining -= filterSamplesRead;
                offset += filterSamplesRead;
            }

            return count - samplesRemaining;
        }

        private void UpdateCoefficients(float filterSample)
        {
            filterSample = GeneralMath.Clamp(filterSample, -1f, 1f);

            if (lastFilterSample == filterSample)
            {
                return;
            }

            lastFilterSample = filterSample;

            double k = Math.Tan(freqMid + freqFactor * filterSample);
            double norm = 1.0 / (1.0 + k / Q + k * k);

            switch (filterType)
            {
                case FilterType.HighPass:
                    a0 = 1.0 * norm;
                    a1 = -2.0 * a0;
                    a2 = a0;
                    b1 = 2.0 * (k * k - 1.0) * norm;
                    b2 = (1.0 - k / Q + k * k) * norm;
                    break;

                case FilterType.LowPass:
                    a0 = k * k * norm;
                    a1 = 2.0 * a0;
                    a2 = a0;
                    b1 = 2.0 * (k * k - 1.0) * norm;
                    b2 = (1.0 - k / Q + k * k) * norm;
                    break;

                case FilterType.BandPass:
                    a0 = k / Q * norm;
                    a1 = 0.0;
                    a2 = -a0;
                    b1 = 2.0 * (k * k - 1.0) * norm;
                    b2 = (1.0 - k / Q + k * k) * norm;
                    break;

                default:
                    throw new Exception($"Unrecognized FilterType: {filterType}");
            }
        }

        private float ProcessSample(float sample)
        {
            double z0 = sample * a0 + Z1;
            Z1 = sample * a1 + Z2 - b1 * z0;
            Z2 = sample * a2 - b2 * z0;
            return (float)z0;
        }

        public override void Seek(int position)
        {
            stream.Seek(position);
            filterEnvelope.Seek(position);

            Z1 = 0.0;
            Z2 = 0.0;

            lastFilterSample = float.NaN;
        }

        public override void Reset()
        {
            stream.Reset();
            filterEnvelope.Reset();

            Z1 = 0.0;
            Z2 = 0.0;

            lastFilterSample = float.NaN;
        }
    }
}

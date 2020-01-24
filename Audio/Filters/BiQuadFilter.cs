using System;
using System.Collections.Generic;

using System.Linq;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// This implementation is based on the one found in CSCore, which in turn was 
    /// based on http://www.earlevel.com/main/2011/01/02/biquad-formulas/
    /// </summary>
    public class BiQuadFilter : SimpleBGCFilter
    {
        private readonly double A0;
        private readonly double A1;
        private readonly double A2;
        private readonly double B1;
        private readonly double B2;

        private double Z1;
        private double Z2;

        private readonly TransformRMSBehavior rmsBehavior;

        public override int Channels => 1;

        public override int TotalSamples => stream.ChannelSamples;

        public override int ChannelSamples => stream.ChannelSamples;

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
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return _channelRMS;
        }

        private BiQuadFilter(
            IBGCStream stream,
            double a0,
            double a1,
            double a2,
            double b1,
            double b2,
            TransformRMSBehavior rmsBehavior)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException($"BiQuad Filter requires a mono input stream. Input stream has {stream.Channels} channels.");
            }

            A0 = a0;
            A1 = a1;
            A2 = a2;
            B1 = b1;
            B2 = b2;

            this.rmsBehavior = rmsBehavior;
        }

        public static BiQuadFilter BandpassFilter(
            IBGCStream stream,
            double centralFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            if (double.IsNaN(qFactor))
            {
                qFactor = 1.0 / Math.Sqrt(2.0);
            }

            double k = Math.Tan(Math.PI * centralFrequency / stream.SamplingRate);
            double norm = 1.0 / (1.0 + k / qFactor + k * k);
            double a0 = k / qFactor * norm;
            double a1 = 0.0;
            double a2 = -a0;
            double b1 = 2.0 * (k * k - 1.0) * norm;
            double b2 = (1.0 - k / qFactor + k * k) * norm;

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public static BiQuadFilter HighpassFilter(
            IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            if (double.IsNaN(qFactor))
            {
                qFactor = 1.0 / Math.Sqrt(2.0);
            }

            double k = Math.Tan(Math.PI * criticalFrequency / stream.SamplingRate);
            double norm = 1.0 / (1.0 + k / qFactor + k * k);
            double a0 = 1.0 * norm;
            double a1 = -2.0 * a0;
            double a2 = a0;
            double b1 = 2.0 * (k * k - 1.0) * norm;
            double b2 = (1.0 - k / qFactor + k * k) * norm;

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public static BiQuadFilter LowpassFilter(
            IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            if (double.IsNaN(qFactor))
            {
                qFactor = 1.0 / Math.Sqrt(2.0);
            }

            double k = Math.Tan(Math.PI * criticalFrequency / stream.SamplingRate);
            double norm = 1.0 / (1.0 + k / qFactor + k * k);
            double a0 = k * k * norm;
            double a1 = 2.0 * a0;
            double a2 = a0;
            double b1 = 2.0 * (k * k - 1.0) * norm;
            double b2 = (1.0 - k / qFactor + k * k) * norm;

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public static BiQuadFilter NotchFilter(
            IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            if (double.IsNaN(qFactor))
            {
                qFactor = 1.0 / Math.Sqrt(2.0);
            }

            double k = Math.Tan(Math.PI * criticalFrequency / stream.SamplingRate);
            double norm = 1.0 / (1.0 + k / qFactor + k * k);
            double a0 = (1.0 + k * k) * norm;
            double a1 = 2.0 * (k * k - 1.0) * norm;
            double a2 = a0;
            double b1 = a1;
            double b2 = (1.0 - k / qFactor + k * k) * norm;

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public static BiQuadFilter LowShelfFilter(
            IBGCStream stream,
            double criticalFrequency,
            double dbGain,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            double k = Math.Tan(Math.PI * criticalFrequency / stream.SamplingRate);
            double gainFactor = Math.Pow(10.0, Math.Abs(dbGain) / 20.0);

            double norm, a0, a1, a2, b1, b2;

            if (dbGain >= 0.0)
            {
                //Boost
                norm = 1.0 / (1.0 + Math.Sqrt(2.0) * k + k * k);
                a0 = (1.0 + Math.Sqrt(2.0 * gainFactor) * k + gainFactor * k * k) * norm;
                a1 = 2.0 * (gainFactor * k * k - 1.0) * norm;
                a2 = (1.0 - Math.Sqrt(2.0 * gainFactor) * k + gainFactor * k * k) * norm;
                b1 = 2.0 * (k * k - 1.0) * norm;
                b2 = (1.0 - Math.Sqrt(2.0) * k + k * k) * norm;
            }
            else
            {
                //Cut
                norm = 1.0 / (1.0 + Math.Sqrt(2.0 * gainFactor) * k + gainFactor * k * k);
                a0 = (1.0 + Math.Sqrt(2.0) * k + k * k) * norm;
                a1 = 2.0 * (k * k - 1.0) * norm;
                a2 = (1.0 - Math.Sqrt(2.0) * k + k * k) * norm;
                b1 = 2.0 * (gainFactor * k * k - 1.0) * norm;
                b2 = (1.0 - Math.Sqrt(2.0 * gainFactor) * k + gainFactor * k * k) * norm;
            }

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public static BiQuadFilter HighShelfFilter(
            IBGCStream stream,
            double criticalFrequency,
            double dbGain,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            double k = Math.Tan(Math.PI * criticalFrequency / stream.SamplingRate);
            double gainFactor = Math.Pow(10.0, Math.Abs(dbGain) / 20.0);

            double norm, a0, a1, a2, b1, b2;

            if (dbGain >= 0.0)
            {
                //Boost
                norm = 1.0 / (1.0 + Math.Sqrt(2.0) * k + k * k);
                a0 = (gainFactor + Math.Sqrt(2.0 * gainFactor) * k + k * k) * norm;
                a1 = 2.0 * (k * k - gainFactor) * norm;
                a2 = (gainFactor - Math.Sqrt(2.0 * gainFactor) * k + k * k) * norm;
                b1 = 2.0 * (k * k - 1.0) * norm;
                b2 = (1.0 - Math.Sqrt(2.0) * k + k * k) * norm;
            }
            else
            {
                //Cut
                norm = 1.0 / (gainFactor + Math.Sqrt(2.0 * gainFactor) * k + k * k);
                a0 = (1.0 + Math.Sqrt(2.0) * k + k * k) * norm;
                a1 = 2.0 * (k * k - 1.0) * norm;
                a2 = (1.0 - Math.Sqrt(2.0) * k + k * k) * norm;
                b1 = 2.0 * (k * k - gainFactor) * norm;
                b2 = (gainFactor - Math.Sqrt(2.0 * gainFactor) * k + k * k) * norm;
            }

            return new BiQuadFilter(stream, a0, a1, a2, b1, b2, rmsBehavior);
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRead = stream.Read(data, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                data[offset + i] = ProcessSample(data[offset + i]);
            }

            return samplesRead;
        }

        public override void Seek(int position)
        {
            base.Seek(position);

            Z1 = 0;
            Z2 = 0;
        }

        public override void Reset()
        {
            base.Reset();

            Z1 = 0;
            Z2 = 0;
        }

        public float ProcessSample(float sample)
        {
            double z0 = sample * A0 + Z1;
            Z1 = sample * A1 + Z2 - B1 * z0;
            Z2 = sample * A2 - B2 * z0;
            return (float)z0;
        }
    }
}

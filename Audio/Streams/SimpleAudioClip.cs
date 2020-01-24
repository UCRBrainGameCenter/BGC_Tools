using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Simplest implementation of a samplebuffer-based stream.
    /// </summary>
    public class SimpleAudioClip : BGCAudioClip
    {
        public int Position
        {
            get => RawPosition / Channels;
            set => RawPosition = value * Channels;
        }

        public int RawPosition { get; private set; } = 0;

        public override int TotalSamples => Samples.Length;
        public override int ChannelSamples => Samples.Length / Channels;

        public float[] Samples { get; }

        private readonly int _channels;
        public override int Channels => _channels;

        private int RemainingChannelSamples => RemainingTotalSamples / Channels;
        private int RemainingTotalSamples => GeneralMath.Clamp(Samples.Length - RawPosition, 0, Samples.Length);

        public SimpleAudioClip(float[] samples, int channels)
        {
            Samples = samples;
            _channels = channels;
        }

        public override int Read(float[] data, int offset, int count)
        {
            //Just directly copy samples
            int copyLength = Math.Min(RemainingTotalSamples, count);

            Array.Copy(
                sourceArray: Samples,
                sourceIndex: RawPosition,
                destinationArray: data,
                destinationIndex: offset,
                length: copyLength);

            RawPosition += copyLength;

            return copyLength;
        }

        public override void Seek(int position) =>
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);

        public override void Reset() => RawPosition = 0;

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                double[] rms = new double[Channels];

                for (int i = 0; i < TotalSamples; i++)
                {
                    rms[i % Channels] += Samples[i] * Samples[i];
                }

                for (int i = 0; i < Channels; i++)
                {
                    rms[i] = Math.Sqrt(rms[i] / ChannelSamples);
                }

                _channelRMS = rms;
            }

            return _channelRMS;
        }
    }
}

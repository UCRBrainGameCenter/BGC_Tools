using System;
using System.Collections.Generic;
using BGC.Mathematics;
using BGC.DataStructures.Generic;

namespace BGC.Audio.Filters
{
    public class AllPassFilter : SimpleBGCFilter
    {
        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        public override int ChannelSamples => stream.ChannelSamples + 2 * delay;

        private readonly TransformRMSBehavior rmsBehavior;
        private readonly Complex64 coeff;
        private readonly Complex64 coeffConj;
        private readonly int delay;
        private readonly RingBuffer<float> inputBuffer;
        private readonly RingBuffer<Complex64> outputBuffer;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        private int position = 0;

        public AllPassFilter(
            IBGCStream stream,
            in Complex64 coeff,
            int delay,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"AllPass Filter requires a mono input stream.  Input stream has {stream.Channels} channels");
            }

            this.delay = delay;

            if (coeff.MagnitudeSquared <= 1f)
            {
                this.coeff = coeff;
            }
            else
            {
                this.coeff = coeff / coeff.Magnitude;
            }

            coeffConj = coeff.Conjugate();

            inputBuffer = new RingBuffer<float>(delay);
            outputBuffer = new RingBuffer<Complex64>(delay);

            inputBuffer.ZeroOut();
            outputBuffer.ZeroOut();

            this.rmsBehavior = rmsBehavior;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesRemaining = count;

            if (position < stream.ChannelSamples)
            {
                int samplesRead = stream.Read(data, offset, count);

                for (int i = 0; i < samplesRead; i++)
                {
                    data[offset + i] = ProcessSample(data[offset + i]);
                }

                position += samplesRead;

                samplesRemaining -= samplesRead;
                offset += samplesRead;
            }

            if (samplesRemaining > 0)
            {
                int tailReadCount = Math.Min(samplesRemaining, ChannelSamples - position);

                for (int i = 0; i < tailReadCount; i++)
                {
                    data[offset + i] = ProcessSample(0f);
                }

                position += tailReadCount;
                samplesRemaining -= tailReadCount;
            }

            return count - samplesRemaining;
        }

        private float ProcessSample(float sample)
        {
            Complex64 newOutput = coeffConj * sample + inputBuffer.Tail - coeff * outputBuffer.Tail;
            inputBuffer.Push(sample);
            outputBuffer.Push(newOutput);

            return (float)newOutput.Real;
        }

        public override void Seek(int position)
        {
            //Ensure position is in range
            position = GeneralMath.Clamp(position, 0, ChannelSamples);

            inputBuffer.ZeroOut();
            outputBuffer.ZeroOut();

            //Give ourselves 2 delay cycles to converge to approximate proper state
            int seekPosition = GeneralMath.Clamp(position - 2 * delay, 0, stream.ChannelSamples);
            stream.Seek(seekPosition);
            int remainingSamples = position - seekPosition;
            int samplesRead;

            do
            {
                int samplesToRead = Math.Min(remainingSamples, BUFFER_SIZE);
                samplesRead = stream.Read(buffer, 0, samplesToRead);

                for (int i = 0; i < samplesRead; i++)
                {
                    ProcessSample(buffer[i]);
                }

                remainingSamples -= samplesRead;

            }
            while (samplesRead > 0 && remainingSamples > 0);

            //Handle tail samples
            for (int i = 0; i < remainingSamples; i++)
            {
                ProcessSample(0f);
            }

            this.position = position;
        }

        public override void Reset()
        {
            inputBuffer.ZeroOut();
            outputBuffer.ZeroOut();

            stream.Reset();

            position = 0;
        }

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
    }
}

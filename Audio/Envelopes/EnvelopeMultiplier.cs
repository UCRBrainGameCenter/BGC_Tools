using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Envelopes
{
    /// <summary>
    /// Creates an envelope that is the sample-wise product of the internal envelopes
    /// </summary>
    public class EnvelopeMultiplier : BGCEnvelopeFilter
    {
        private readonly List<IBGCEnvelopeStream> streams = new List<IBGCEnvelopeStream>();
        public override IEnumerable<IBGCEnvelopeStream> InternalStreams => streams;

        private int _sampleCount = 0;
        public override int Samples => _sampleCount;

        private float _samplingRate;
        public override float SamplingRate => _samplingRate;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public EnvelopeMultiplier()
        {
            UpdateStats();
        }

        public EnvelopeMultiplier(params IBGCEnvelopeStream[] streams)
        {
            AddStreams(streams);
        }

        public EnvelopeMultiplier(IEnumerable<IBGCEnvelopeStream> streams)
        {
            AddStreams(streams);
        }

        public void AddStream(IBGCEnvelopeStream stream)
        {
            streams.Add(stream);
            UpdateStats();
        }

        public void AddStreams(IEnumerable<IBGCEnvelopeStream> streams)
        {
            this.streams.AddRange(streams);
            UpdateStats();
        }

        public override void Reset() => streams.ForEach(x => x.Reset());

        public override bool HasMoreSamples() => streams.All(x => x.HasMoreSamples());

        public override float ReadNextSample()
        {
            if (HasMoreSamples())
            {
                return streams.Select(x => x.ReadNextSample()).Aggregate(1f, (acc, val) => acc * val);
            }

            //Hit the end
            return 0f;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int maxRemainingSamples = 0;

            for (int i = 0; i < count; i++)
            {
                data[offset + i] = 1f;
            }

            foreach (IBGCEnvelopeStream stream in streams)
            {
                int streamRemainingSamples = count - maxRemainingSamples;
                int streamOffset = offset;

                while (streamRemainingSamples > 0)
                {
                    int maxRead = Math.Min(BUFFER_SIZE, streamRemainingSamples);
                    int streamReadSamples = stream.Read(buffer, 0, maxRead);

                    if (streamReadSamples == 0)
                    {
                        //Done with this stream
                        break;
                    }

                    for (int i = 0; i < streamReadSamples; i++)
                    {
                        data[streamOffset + i] *= buffer[i];
                    }

                    streamOffset += streamReadSamples;
                    streamRemainingSamples -= streamReadSamples;
                }

                maxRemainingSamples = Math.Max(maxRemainingSamples, streamRemainingSamples);
            }

            if (maxRemainingSamples == 0)
            {
                return count;
            }

            int readSamples = count - maxRemainingSamples;

            Array.Clear(data, offset + readSamples, maxRemainingSamples);

            return readSamples;
        }

        public override void Seek(int position) => streams.ForEach(x => x.Seek(position));

        private void UpdateStats()
        {
            if (streams.Count > 0)
            {
                IEnumerable<float> samplingRates = streams.Select(x => x.SamplingRate);
                _samplingRate = samplingRates.Max();

                if (_samplingRate != samplingRates.Min())
                {
                    throw new StreamCompositionException("EnvelopeMultiplier requires all streams have the same samplingRate.");
                }

                _sampleCount = streams.Select(x => x.Samples).Max();
            }
            else
            {
                _sampleCount = 0;
                _samplingRate = 44100f;
            }
        }
    }
}

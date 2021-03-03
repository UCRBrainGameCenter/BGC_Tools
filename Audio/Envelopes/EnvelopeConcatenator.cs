using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio.Envelopes
{
    public class EnvelopeConcatenator : BGCEnvelopeFilter
    {
        private readonly List<IBGCEnvelopeStream> streams = new List<IBGCEnvelopeStream>();
        public override IEnumerable<IBGCEnvelopeStream> InternalStreams => streams;

        private int _sampleCount = 0;
        public override int Samples => _sampleCount;

        private float _samplingRate;
        public override float SamplingRate => _samplingRate;

        public int CurrentClipIndex { get; private set; }
        public int Position { get; private set; }

        public EnvelopeConcatenator()
        {
            UpdateStats();
        }

        public EnvelopeConcatenator(params IBGCEnvelopeStream[] streams)
        {
            AddStreams(streams);
        }

        public EnvelopeConcatenator(IEnumerable<IBGCEnvelopeStream> streams)
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

        public override void Reset()
        {
            CurrentClipIndex = 0;
            Position = 0;
            streams.ForEach(x => x.Reset());
        }

        public override bool HasMoreSamples() => Position < Samples;

        public override float ReadNextSample()
        {
            float value = 0f;

            while (CurrentClipIndex < streams.Count)
            {
                IBGCEnvelopeStream stream = streams[CurrentClipIndex];

                if (!stream.HasMoreSamples())
                {
                    CurrentClipIndex++;
                    if (CurrentClipIndex < streams.Count)
                    {
                        //Reset on advancing allows a concatenator to hold multiple 
                        //copies of the same clip
                        streams[CurrentClipIndex].Reset();
                    }
                    continue;
                }

                value = stream.ReadNextSample();
                Position++;
                break;
            }

            //Hit the end
            return value;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int remainingSamples = count;

            while (remainingSamples > 0)
            {
                if (CurrentClipIndex >= streams.Count)
                {
                    //Hit the end
                    break;
                }

                IBGCEnvelopeStream stream = streams[CurrentClipIndex];

                int readSamples = stream.Read(data, offset, remainingSamples);

                remainingSamples -= readSamples;
                offset += readSamples;
                Position += readSamples;

                if (readSamples <= 0)
                {
                    CurrentClipIndex++;

                    if (CurrentClipIndex < streams.Count)
                    {
                        //Reset on advancing allows a concatenator to hold multiple 
                        //copies of the same clip
                        streams[CurrentClipIndex].Reset();
                    }
                }
            }

            return count - remainingSamples;
        }

        public override void Seek(int position)
        {
            Position = position;
            CurrentClipIndex = streams.Count;

            for (int i = 0; i < streams.Count; i++)
            {
                IBGCEnvelopeStream clip = streams[i];
                if (position > 0)
                {
                    //Seek
                    if (position > clip.Samples)
                    {
                        clip.Seek(clip.Samples);
                        position -= clip.Samples;
                    }
                    else
                    {
                        clip.Seek(position);
                        position = 0;
                        CurrentClipIndex = i;
                    }
                }
                else
                {
                    clip.Reset();
                }
            }
        }

        private void UpdateStats()
        {
            if (streams.Count > 0)
            {
                IEnumerable<float> samplingRates = streams.Select(x => x.SamplingRate);
                _samplingRate = samplingRates.Max();

                if (_samplingRate != samplingRates.Min())
                {
                    throw new StreamCompositionException("EnvelopeConcatenator requires all streams have the same samplingRate.");
                }


                if (streams.Any(x => x.Samples == int.MaxValue))
                {
                    _sampleCount = int.MaxValue;
                }
                else
                {
                    _sampleCount = streams.Select(x => x.Samples).Sum();
                }
            }
            else
            {
                _sampleCount = 0;
                _samplingRate = 44100f;
            }
        }
    }
}

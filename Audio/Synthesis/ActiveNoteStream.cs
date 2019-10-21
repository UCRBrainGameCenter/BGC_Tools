using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Audio.Filters;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Stores and renders a set of active notes.
    /// Generally meant to interact with Active ADSR Envelopes.
    /// </summary>
    public class ActiveNoteStream : BGCFilter
    {
        private List<IBGCStream> pendingDeletions = new List<IBGCStream>();
        private List<IBGCStream> collisionNotes = new List<IBGCStream>();
        private readonly Dictionary<int, IBGCStream> streams = new Dictionary<int, IBGCStream>();
        public override IEnumerable<IBGCStream> InternalStreams => streams.Values;

        public override int Channels => 1;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        public override float SamplingRate => 44100f;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        private static readonly double rms = Math.Sqrt(0.5);

        public ActiveNoteStream()
        {
        }

        public void AddStream(int key, IBGCStream stream)
        {
            if (stream == null)
            {
                Debug.Log("Added null stream");
                return;
            }

            if (streams.ContainsKey(key))
            {
                IBGCStream oldStream = streams[key];
                collisionNotes.Add(oldStream);

                if (oldStream is IADSR asdr)
                {
                    asdr.TriggerRelease(true);
                }

                RemoveStream(key);
            }

            streams.Add(key, stream);
        }

        public bool RemoveStream(IBGCStream stream)
        {
            if (streams.ContainsValue(stream))
            {
                int key = streams.FirstOrDefault(x => x.Value == stream).Key;
                streams.Remove(key);
                return true;
            }

            return false;
        }

        public bool RemoveStream(int key)
        {
            return streams.Remove(key);
        }

        public override void Reset()
        {
            foreach (IBGCStream stream in streams.Values)
            {
                stream.Reset();
            }
        }

        public void Clear()
        {
            collisionNotes.Clear();
            pendingDeletions.Clear();
            streams.Clear();
        }

        public override int Read(float[] data, int offset, int count)
        {
            Array.Clear(data, offset, count);

            foreach (IBGCStream stream in pendingDeletions)
            {
                RemoveStream(stream);
            }

            foreach (IBGCStream stream in streams.Values.Concat(collisionNotes))
            {
                int streamRemainingSamples = count;
                int streamOffset = offset;

                while (streamRemainingSamples > 0)
                {
                    int maxRead = Math.Min(BUFFER_SIZE, streamRemainingSamples);
                    int streamReadSamples = stream.Read(buffer, 0, maxRead);

                    if (streamReadSamples == 0)
                    {
                        pendingDeletions.Add(stream);
                        //Done with this stream
                        break;
                    }

                    for (int i = 0; i < streamReadSamples; i++)
                    {
                        data[streamOffset + i] += buffer[i];
                    }

                    streamOffset += streamReadSamples;
                    streamRemainingSamples -= streamReadSamples;
                }
            }

            foreach (IBGCStream stream in pendingDeletions)
            {
                RemoveStream(stream);
                collisionNotes.Remove(stream);
            }

            pendingDeletions.Clear();

            return count;
        }

        public bool Release(int key, bool immediate = false)
        {
            if (streams.ContainsKey(key))
            {
                if (streams[key] is ADSREnvelope activeEnvelope)
                {
                    activeEnvelope.TriggerRelease(immediate);
                }
                else
                {
                    RemoveStream(key);
                }

                return true;
            }

            return false;
        }

        public override void Seek(int position)
        {
            foreach (IBGCStream stream in streams.Values)
            {
                stream.Seek(position);
            }
        }

        //RMS will pretend to be one 1.0 amplitude sine wave
        public override IEnumerable<double> GetChannelRMS()
        {
            yield return rms;
        }
    }
}

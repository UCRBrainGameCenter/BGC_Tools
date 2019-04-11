using System;
using System.Collections.Generic;

namespace BGC.Audio.Envelopes
{
    /// <summary>
    /// Filters represent transformations of the BGCAudioStreams.
    /// Like Spatialization, windowing, and Carlile-Filtering
    /// </summary>
    public abstract class BGCEnvelopeFilter : IBGCEnvelopeStream
    {
        public abstract int Samples { get; }

        public abstract float SamplingRate { get; }

        public abstract bool HasMoreSamples();
        public abstract int Read(float[] data, int offset, int count);
        public abstract float ReadNextSample();
        public abstract void Reset();
        public abstract void Seek(int position);

        public abstract IEnumerable<IBGCEnvelopeStream> InternalStreams { get; }

        protected bool initialized = false;

        protected virtual void _Initialize() { }

        public virtual void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                foreach (IBGCEnvelopeStream stream in InternalStreams)
                {
                    stream.Initialize();
                }

                _Initialize();
            }
        }
    }
}

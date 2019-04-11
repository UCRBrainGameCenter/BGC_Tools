using System;
using System.Collections.Generic;

namespace BGC.Audio.Envelopes
{
    public abstract class BGCEnvelopeStream : IBGCEnvelopeStream
    {
        public abstract int Samples { get; }

        public abstract float SamplingRate { get; }

        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                _Initialize();
            }
        }

        protected bool initialized = false;
        protected virtual void _Initialize() { }

        public abstract int Read(float[] data, int offset, int count);

        public abstract void Reset();

        public abstract bool HasMoreSamples();

        public abstract float ReadNextSample();

        public abstract void Seek(int position);
    }
}

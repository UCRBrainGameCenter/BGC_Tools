using System;
using System.Collections.Generic;

namespace BGC.Audio
{
    public abstract class BGCStream : IBGCStream
    {
        public abstract int Channels { get; }

        public abstract int TotalSamples { get; }

        public abstract int ChannelSamples { get; }

        public abstract float SamplingRate { get; }

        public abstract IEnumerable<double> GetChannelRMS();

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

        public abstract void Seek(int position);

        public virtual void Dispose()
        {
        }
    }
}

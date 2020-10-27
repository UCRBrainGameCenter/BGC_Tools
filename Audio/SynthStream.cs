using System.Collections.Generic;

namespace BGC.Audio
{
    public abstract class SynthStream : IBGCStream
    {
        public abstract int Channels { get; }

        public abstract int TotalSamples { get; }

        public abstract int ChannelSamples { get; }

        public float SamplingRate => 44100f;

        public abstract IEnumerable<double> GetChannelRMS();

        public abstract int Read(float[] data, int offset, int count);

        public abstract void Reset();

        public abstract void Seek(int position);

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

        public virtual void Dispose()
        {
        }
    }
}

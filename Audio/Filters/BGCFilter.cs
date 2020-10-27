using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Filters represent transformations of the BGCAudioStreams.
    /// Like Spatialization, windowing, and Carlile-Filtering
    /// </summary>
    public abstract class BGCFilter : IBGCStream
    {
        public abstract int Channels { get; }

        public abstract int TotalSamples { get; }
        public abstract int ChannelSamples { get; }

        public abstract float SamplingRate { get; }

        public abstract int Read(float[] data, int offset, int count);
        public abstract void Reset();
        public abstract void Seek(int position);

        public abstract IEnumerable<double> GetChannelRMS();
        public abstract IEnumerable<IBGCStream> InternalStreams { get; }

        protected bool initialized = false;

        protected virtual void _Initialize() { }

        public virtual void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                foreach (IBGCStream stream in InternalStreams)
                {
                    stream.Initialize();
                }

                _Initialize();
            }
        }

        public virtual void Dispose()
        {
            foreach (IBGCStream stream in InternalStreams)
            {
                stream.Dispose();
            }
        }
    }
}

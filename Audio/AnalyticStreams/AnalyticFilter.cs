using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// Filters represent transformations of the AnalyticStream.
    /// </summary>
    public abstract class AnalyticFilter : IAnalyticStream
    {
        public abstract int Samples { get; }

        public abstract double SamplingRate { get; }

        public abstract int Read(Complex64[] data, int offset, int count);
        public abstract void Reset();
        public abstract void Seek(int position);

        public abstract double GetRMS();
        public abstract IEnumerable<IAnalyticStream> InternalStreams { get; }

        protected bool initialized = false;

        protected virtual void _Initialize() { }

        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                foreach (IAnalyticStream stream in InternalStreams)
                {
                    stream.Initialize();
                }

                _Initialize();
            }
        }

        public abstract void Dispose();
    }
}

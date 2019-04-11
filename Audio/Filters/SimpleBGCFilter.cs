using System;
using System.Collections.Generic;

namespace BGC.Audio.Filters
{
    public abstract class SimpleBGCFilter : BGCFilter
    {
        protected readonly IBGCStream stream;

        public override IEnumerable<IBGCStream> InternalStreams
        {
            get { yield return stream; }
        }

        public override float SamplingRate => stream.SamplingRate;

        public SimpleBGCFilter(IBGCStream stream)
        {
            this.stream = stream;
        }

        public override void Seek(int position) => stream.Seek(position);

        public override void Reset() => stream.Reset();

        public override void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                stream.Initialize();
                _Initialize();
            }
        }
    }
}

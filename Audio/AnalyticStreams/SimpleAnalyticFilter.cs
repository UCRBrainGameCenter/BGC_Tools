using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    public abstract class SimpleAnalyticFilter : AnalyticFilter
    {
        protected readonly IAnalyticStream stream;

        public override IEnumerable<IAnalyticStream> InternalStreams => new IAnalyticStream[] { stream };

        public override double SamplingRate => stream.SamplingRate;

        public SimpleAnalyticFilter(IAnalyticStream stream)
        {
            this.stream = stream;
        }

        public override void Seek(int position) => stream.Seek(position);

        public override void Reset() => stream.Reset();

        public override void Dispose()
        {
            stream?.Dispose();
        }
    }
}

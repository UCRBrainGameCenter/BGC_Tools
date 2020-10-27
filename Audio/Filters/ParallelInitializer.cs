using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Runs the initialization methods of the underlying streams in a parallel process.
    /// </summary>
    public class ParallelInitializer : IBGCStream
    {
        private readonly IBGCStream stream;

        public int Channels => stream.Channels;

        public int TotalSamples => stream.TotalSamples;
        public int ChannelSamples => stream.ChannelSamples;

        public float SamplingRate => stream.SamplingRate;

        private readonly object initLock = new object();
        private Task initializationTask = null;

        private bool initializationStarted = false;
        private bool initializationFinished = false;

        public ParallelInitializer(IBGCStream stream)
        {
            this.stream = stream;
        }

        public void Seek(int position) => stream.Seek(position);

        public void Reset() => stream.Reset();

        public int Read(float[] data, int offset, int count)
        {
            if (!initializationFinished)
            {
                InitializeNow();
            }

            return stream.Read(data, offset, count);
        }

        public IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();

        public void Initialize()
        {
            if (initializationStarted)
            {
                return;
            }

            lock (initLock)
            {
                if (!initializationStarted)
                {
                    initializationStarted = true;
                    initializationTask = Task.Run(stream.Initialize);
                    initializationFinished = true;
                }
            }
        }

        public void InitializeNow()
        {
            if (initializationFinished)
            {
                return;
            }

            if (initializationStarted && initializationTask != null)
            {
                initializationTask.Wait();
                return;
            }

            lock (initLock)
            {
                if (!initializationStarted)
                {
                    initializationStarted = true;
                    initializationTask = Task.Run(stream.Initialize);
                    initializationFinished = true;
                }
            }

            initializationTask?.Wait();
        }

        public void Dispose()
        {
            stream?.Dispose();
        }
    }
}

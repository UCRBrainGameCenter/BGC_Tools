using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    public interface IAnalyticStream : IDisposable
    {
        /// <summary>
        /// The total number of Samples of this AnalyticStream
        /// </summary>
        int Samples { get; }

        /// <summary>
        /// The sampling rate of the Stream
        /// </summary>
        double SamplingRate { get; }

        /// <summary>
        /// Perform any calculations necessary to prepare the Stream
        /// </summary>
        void Initialize();

        /// <summary>
        /// Copy count samples into the Data buffer, starting at offset.
        /// </summary>
        /// <returns>The number of samples copied</returns>
        int Read(Complex64[] data, int offset, int count);

        /// <summary>
        /// Sets this internal state of this stream to the initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Seek to the indicated position in the stream
        /// </summary>
        void Seek(int position);

        /// <summary>
        /// The RMS amplitude of the stream (real component)
        /// </summary>
        double GetRMS();
    }
}

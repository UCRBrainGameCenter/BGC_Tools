using System;
using System.Collections.Generic;

namespace BGC.Audio
{
    public interface IBGCStream : IDisposable
    {
        /// <summary>
        /// The number of underlying Channels of this BGCStream
        /// </summary>
        int Channels { get; }

        /// <summary>
        /// The total number of Samples of this BGCStream
        /// </summary>
        int TotalSamples { get; }

        /// <summary>
        /// The number of Samples of each channel for this BGCStream
        /// </summary>
        int ChannelSamples { get; }

        /// <summary>
        /// The sampling rate of the Stream
        /// </summary>
        float SamplingRate { get; }

        /// <summary>
        /// Perform any calculations necessary to prepare the Stream
        /// </summary>
        void Initialize();

        /// <summary>
        /// Copy count samples into the Data buffer, starting at offset.
        /// </summary>
        /// <returns>The number of samples copied</returns>
        int Read(float[] data, int offset, int count);

        /// <summary>
        /// Sets this internal state of this stream to the initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Seek to the indicated position in the stream
        /// </summary>
        void Seek(int position);

        /// <summary>
        /// The RMS amplitude of each channel
        /// </summary>
        IEnumerable<double> GetChannelRMS();
    }
}

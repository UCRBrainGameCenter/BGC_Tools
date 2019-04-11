using System;
using System.Collections.Generic;

namespace BGC.Audio.Envelopes
{
    public interface IBGCEnvelopeStream
    {
        /// <summary>
        /// The total number of Samples of this BGCEnvelopeStream
        /// </summary>
        int Samples { get; }

        /// <summary>
        /// The sampling rate of the BGCEnvelopeStream
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
        /// Returns if the BGCEnvelopeStream has more samples
        /// </summary>
        bool HasMoreSamples();

        /// <summary>
        /// Returns the next sample in the BGCEnvelopeStream
        /// </summary>
        /// <returns>The next sample</returns>
        float ReadNextSample();

        /// <summary>
        /// Sets this internal state of this stream to the initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Seek to the indicated position in the stream
        /// </summary>
        void Seek(int position);
    }
}

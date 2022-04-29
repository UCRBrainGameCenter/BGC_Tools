using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BGC.Audio
{
    /// <summary>
    /// Ability to programmatically play BGCStreams in a Unity scene, with a
    /// callback on completion.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BGCClipPlayer : MonoBehaviour
    {
        IBGCStream stream = null;
        private bool disposeStream = false;

        private SoundState currentState = SoundState.Stopped;

        private class BufferedData
        {
            public float[] Samples { get; }
            public int Offset { get; set; } = 0;
            public int Size { get; set; } = 0;
            public int NumChannels { get; set; } = 0;
            public bool EndOfStream => Size != Samples.Length || NumChannels == 0;
            public int SamplesRemaining => Size - Offset;
            public bool IsValid => Size > 0 && (NumChannels == 1 || NumChannels == 2);

            public BufferedData()
            {
                Samples = new float[44100 / 2]; // 250ms with 2 channels at 44100hz
            }
        }

        private readonly BufferedData[] buffers = new BufferedData[]
        {
            new BufferedData(),
            new BufferedData(),
        };

        private int bufferIndex = 0;
        private Task fillBufferTask;

        public delegate void PlaybackStopped();
        public PlaybackStopped playbackEndedNotifier;

        private enum SoundState
        {
            Playing,
            Stopped,
            MAX
        }

        public bool IsPlaying => currentState == SoundState.Playing;

        public double Duration => stream?.Duration() ?? 0.0;

        public void SetStream(
            IBGCStream stream,
            bool disposeWhenComplete = false)
        {
            if (disposeStream && this.stream != null && !ReferenceEquals(stream, this.stream))
            {
                this.stream.Dispose();
            }

            if (stream != null)
            {
                int numChannels = stream.Channels;
                if (numChannels != 1 && numChannels != 2)
                {
                    Debug.LogError($"Completely unexpected stream channel count: {numChannels}");
                    if (disposeWhenComplete)
                    {
                        stream.Dispose();
                    }
                    stream = null;
                    disposeWhenComplete = false;
                }
            }

            this.stream = stream;
            disposeStream = disposeWhenComplete;
            currentState = SoundState.Stopped;
            ClearBuffers();
            WarmUpBuffers();
        }

        public virtual void PlayStream(
            IBGCStream stream,
            bool disposeWhenComplete = false)
        {
            SetStream(stream, disposeWhenComplete);
            Play();
        }

        public void Stop(bool invoke = false)
        {
            if (currentState == SoundState.Stopped)
            {
                return;
            }

            currentState = SoundState.Stopped;

            PlaybackEnded();

            if (invoke)
            {
                playbackEndedNotifier?.Invoke();
            }
        }

        public void Play()
        {
            if (stream == null)
            {
                Debug.LogError($"AudioClip was null");
            }

            currentState = SoundState.Playing;
        }

        public void ResetClip()
        {
            ClearBuffers();
            stream?.Reset();
            WarmUpBuffers();
        }

        public void Clear()
        {
            Stop(false);
            if (disposeStream && stream != null)
            {
                stream.Dispose();
            }
            stream = null;
            disposeStream = false;
            playbackEndedNotifier = null;
            ClearBuffers();
        }

        protected virtual void PlaybackEnded() { }

        void OnAudioFilterRead(float[] data, int dstNumChannels)
        {
            if (currentState != SoundState.Playing)
            {
                return;
            }

            CopyBufferedAudio(data, 0, dstNumChannels);
        }

        /// <summary>
        /// Copies the double-buffered audio into the destination buffer, a starting new
        /// async buffering task if necessary.
        /// This function calls itself recursively if there is not enough data in the current
        /// buffer index.
        /// </summary>
        /// <param name="dstBuffer">The buffer to copy to. Channel data is interleaved.</param>
        /// <param name="dstBufferOffset">The index into the buffer to begin copying to.</param>
        /// <param name="dstNumChannels">The number of interleaved channels in the buffer. Must be set to 1 or 2.</param>
        private void CopyBufferedAudio(
            float[] dstBuffer,
            int dstBufferOffset,
            int dstNumChannels)
        {
            Debug.Assert(dstNumChannels > 0);

            int dstSamplesToCopyPerChannel = (dstBuffer.Length - dstBufferOffset) / dstNumChannels;
            if (dstSamplesToCopyPerChannel == 0)
            {
                // 0 bytes to copy, so do nothing.
                return;
            }

            if (dstNumChannels != 1 && dstNumChannels != 2)
            {
                Debug.LogError($"Completely unexpected Player channel count: {dstNumChannels}");
                return;
            }

            bool endOfStream = false;
            BufferedData srcBuffer = buffers[bufferIndex];
            if (!srcBuffer.IsValid)
            {
                ClearSamples(dstBuffer, dstBufferOffset);
                endOfStream = true;
            }
            else
            {
                int srcNumChannels = srcBuffer.NumChannels;
                if (dstNumChannels != srcNumChannels)
                {
                    Debug.LogError($"Stream/Player channel mismatch: Src channels = {srcNumChannels}, Dst channels = {dstNumChannels}");
                }

                int srcSamplesAvailablePerChannel = srcBuffer.SamplesRemaining / srcNumChannels;
                int numSamplesToCopyPerChannel = srcSamplesAvailablePerChannel > dstSamplesToCopyPerChannel ? dstSamplesToCopyPerChannel : srcSamplesAvailablePerChannel;
                CopySamples(srcBuffer.Samples, srcBuffer.Offset, srcNumChannels, dstBuffer, dstBufferOffset, dstNumChannels, numSamplesToCopyPerChannel);
                srcBuffer.Offset += numSamplesToCopyPerChannel * srcNumChannels;
                dstSamplesToCopyPerChannel -= numSamplesToCopyPerChannel;
                dstBufferOffset += numSamplesToCopyPerChannel * dstNumChannels;

                if (dstSamplesToCopyPerChannel > 0)
                {
                    if (!srcBuffer.EndOfStream)
                    {
                        // A buffer swap will be required

                        // Unless the stream processing speed is slower than the sample rate, the task should
                        // already by completed by now.
                        WaitForBufferTask();

                        bufferIndex = (bufferIndex + 1) % buffers.Length;
                        srcBuffer = buffers[bufferIndex];

                        if (!srcBuffer.EndOfStream)
                        {
                            StartFillBufferTask();
                        }
                        CopyBufferedAudio(dstBuffer, dstBufferOffset, dstNumChannels);
                    }
                    else
                    {
                        ClearSamples(dstBuffer, dstBufferOffset);
                        endOfStream = true;
                    }
                }
            }

            if (endOfStream)
            {
                currentState = SoundState.Stopped;
                PlaybackEnded();
                playbackEndedNotifier?.Invoke();
            }
        }

        private void CopySamples(
            float[] srcBuffer,
            int srcBufferOffset,
            int srcNumChannels,
            float[] dstBuffer,
            int dstBufferOffset,
            int dstNumChannels,
            int samplesToCopyPerChannel)
        {
            if (srcNumChannels == dstNumChannels)
            {
                Array.Copy(srcBuffer, srcBufferOffset, dstBuffer, dstBufferOffset, samplesToCopyPerChannel * dstNumChannels);
            }
            else if (srcNumChannels == 1 && dstNumChannels == 2)
            {
                for (int i = 0; i < samplesToCopyPerChannel; i++)
                {
                    for (int c = 0; c < dstNumChannels; c++)
                    {
                        dstBuffer[dstBufferOffset + i * dstNumChannels + c] = srcBuffer[srcBufferOffset + i];
                    }
                }
            }
            else if (srcNumChannels == 2 && dstNumChannels == 1)
            {
                for (int i = 0; i < samplesToCopyPerChannel; i++)
                {
                    dstBuffer[dstBufferOffset + i] = srcBuffer[srcBufferOffset + i * 2];
                }
            }
            else
            {
                Debug.LogError($"Unexpected number of src or dst channels: {srcNumChannels} => {dstNumChannels}");
            }
        }

        private void ClearSamples(
            float[] dstBuffer,
            int dstBufferOffset)
        {
            for (int i = dstBufferOffset; i < dstBuffer.Length; i++)
            {
                dstBuffer[i] = 0f;
            }
        }

        private void WaitForBufferTask()
        {
            try
            {
                fillBufferTask?.Wait();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in stream reading task: {e}");
            }
            fillBufferTask = null;
        }

        private void ClearBuffers()
        {
            WaitForBufferTask();

            bufferIndex = 0;
            for (int i = 0; i < buffers.Length; i++)
            {
                buffers[i].Offset = 0;
                buffers[i].Size = 0;
                buffers[i].NumChannels = 0;
            }
        }

        private void WarmUpBuffers()
        {
            if (stream == null)
            {
                return;
            }

            bufferIndex = 0;
            FillBuffer(stream, buffers[0]);
            if (!buffers[0].EndOfStream)
            {
                StartFillBufferTask();
            }
        }

        private void StartFillBufferTask()
        {
            int fillBufferIndex = (bufferIndex + 1) % buffers.Length;
            IBGCStream fillStream = stream;
            fillBufferTask = Task.Run(() => FillBuffer(fillStream, buffers[fillBufferIndex]));
        }

        private static void FillBuffer(
            IBGCStream stream,
            BufferedData buffer)
        {
            buffer.Size = stream.Read(buffer.Samples, 0, buffer.Samples.Length);
            buffer.Offset = 0;
            buffer.NumChannels = stream.Channels;
        }
    }
}

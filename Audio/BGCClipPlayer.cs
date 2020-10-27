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

        public void SetStream(IBGCStream stream, bool disposeWhenComplete = false)
        {
            if (disposeStream && this.stream != null && !ReferenceEquals(stream, this.stream))
            {
                this.stream.Dispose();
            }

            this.stream = stream;
            disposeStream = disposeWhenComplete;
            currentState = SoundState.Stopped;
        }

        public void PlayStream(IBGCStream stream, bool disposeWhenComplete = false)
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

        public void ResetClip() => stream?.Reset();

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
        }

        void OnAudioFilterRead(float[] data, int numchannels)
        {
            if (currentState == SoundState.Playing && stream != null)
            {
                if (numchannels != stream.Channels)
                {
                    Debug.LogError($"Stream/Player channel mismatch: {stream.Channels}, {numchannels}");

                    switch (numchannels)
                    {
                        case 1:
                            //Down-channel
                            stream = stream.IsolateChannel(0);
                            break;

                        case 2:
                            //Up-channel
                            stream = stream.UpChannel();
                            break;

                        default:
                            Debug.LogError($"Completely unexpected Player channel count: {numchannels}");
                            break;
                    }
                }

                int readSamples = stream.Read(data, 0, data.Length);

                if (readSamples < data.Length)
                {
                    currentState = SoundState.Stopped;
                    playbackEndedNotifier?.Invoke();
                }
            }
        }
    }
}

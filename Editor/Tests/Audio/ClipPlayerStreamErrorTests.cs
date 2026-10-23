using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BGC.Audio;

namespace BGC.Tests
{
    /// <summary>
    /// <see cref="BGCClipPlayer.StreamErrorHandler"/>: a stream that throws while it plays (a sound
    /// that can't be built, a broken file) either ends there as silence, when the handler accepts
    /// the error, or throws as it always did, when there is no handler or it declines.
    /// </summary>
    public class ClipPlayerStreamErrorTests
    {
        private const int Frames = 44100;

        /// <summary> Stereo, 1 s of 0.5, that throws on read number <paramref name="failingRead"/> (0-based) </summary>
        private class FailingStream : BGCStream
        {
            private readonly int failingRead;
            private int reads = 0;

            public FailingStream(int failingRead)
            {
                this.failingRead = failingRead;
            }

            public override int Channels => 2;
            public override int ChannelSamples => Frames;
            public override int TotalSamples => Channels * Frames;
            public override float SamplingRate => 44100f;

            public override IEnumerable<double> GetChannelRMS() => new[] { 0.5, 0.5 };
            public override IEnumerable<PresentationConstraints> GetPresentationConstraints() => null;

            public override int Read(float[] data, int offset, int count)
            {
                if (reads++ == failingRead)
                {
                    throw new InvalidOperationException("This sound can't be built");
                }

                for (int i = offset; i < offset + count; i++)
                {
                    data[i] = 0.5f;
                }
                return count;
            }

            public override void Reset() { }
            public override void Seek(int position) { }
        }

        private GameObject host;
        private BGCClipPlayer player;
        private List<Exception> handled;
        private Func<Exception, bool> savedHandler;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("ClipPlayerStreamErrorTests");
            player = host.AddComponent<BGCClipPlayer>();
            handled = new List<Exception>();
            savedHandler = BGCClipPlayer.StreamErrorHandler;
            BGCClipPlayer.StreamErrorHandler = null;
        }

        [TearDown]
        public void TearDown()
        {
            BGCClipPlayer.StreamErrorHandler = savedHandler;
            UnityEngine.Object.DestroyImmediate(host);
        }

        /// <summary> One audio callback, as Unity's audio thread makes it </summary>
        private float[] Pull(int frames)
        {
            float[] data = new float[2 * frames];
            MethodInfo onRead = typeof(BGCClipPlayer).GetMethod("OnAudioFilterRead", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(onRead, "BGCClipPlayer.OnAudioFilterRead not found");
            onRead.Invoke(player, new object[] { data, 2 });
            return data;
        }

        [Test]
        public void FirstReadThrows_NoHandler_ReachesTheCaller()
        {
            Assert.Throws<InvalidOperationException>(() => player.PlayStream(new FailingStream(0)));
        }

        [Test]
        public void FirstReadThrows_HandlerDeclines_ReachesTheCaller()
        {
            BGCClipPlayer.StreamErrorHandler = e => { handled.Add(e); return false; };

            Assert.Throws<InvalidOperationException>(() => player.PlayStream(new FailingStream(0)));
            Assert.AreEqual(1, handled.Count, "The handler wasn't asked");
        }

        [Test]
        public void FirstReadThrows_HandlerAccepts_PlaysSilenceAndEnds()
        {
            BGCClipPlayer.StreamErrorHandler = e => { handled.Add(e); return true; };
            int ended = 0;

            Assert.DoesNotThrow(() => player.PlayStream(new FailingStream(0)));
            player.playbackEndedNotifier = () => ended++;
            float[] played = Pull(1024);

            Assert.AreEqual(1, handled.Count, "The handler wasn't asked exactly once");
            StringAssert.Contains("can't be built", handled[0].Message);
            Assert.IsTrue(played.All(v => v == 0f), "The failed sound wasn't silent");
            Assert.IsFalse(player.IsPlaying, "Playback didn't end");
            Assert.AreEqual(1, ended, "The playback-ended notifier didn't fire once");
        }

        /// <summary>
        /// A failure on the buffering thread, after the first buffer (22050 samples, 11025 frames)
        /// has played: the sound plays up to the failure, then silence, and ends.
        /// </summary>
        [Test]
        public void LaterReadThrows_HandlerAccepts_PlaysUpToTheFailureThenEnds()
        {
            BGCClipPlayer.StreamErrorHandler = e => { handled.Add(e); return true; };
            int ended = 0;

            player.PlayStream(new FailingStream(1));
            player.playbackEndedNotifier = () => ended++;
            float[] played = Pull(Frames);

            const int bufferedFrames = 44100 / 2 / 2;
            Assert.AreEqual(1, handled.Count, "The handler wasn't asked exactly once");
            Assert.IsTrue(played.Take(2 * bufferedFrames).All(v => v == 0.5f), "The first buffer didn't play");
            Assert.IsTrue(played.Skip(2 * bufferedFrames).All(v => v == 0f), "The sound didn't go silent at the failure");
            Assert.IsFalse(player.IsPlaying, "Playback didn't end");
            Assert.AreEqual(1, ended, "The playback-ended notifier didn't fire once");
        }
    }
}

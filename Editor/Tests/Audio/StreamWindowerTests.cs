using System;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;

namespace BGC.Tests
{
    /// <summary>
    /// Regression tests for <see cref="StreamWindower"/> windowing streams that are
    /// shorter than the requested smoothing skirts.
    ///
    /// Background: a stream shorter than (openingSkirt + closingSkirt) must have its
    /// skirts shrunk to fit. An inverted sign in that reduction (regressed when the
    /// single- and two-function windower constructors were consolidated) drove the
    /// closing skirt negative, so <c>Windowing.GetHalfWindow</c> attempted
    /// <c>new float[negative]</c> and threw a message-less <see cref="OverflowException"/>.
    /// The repro in PART is the standard (non-target) interval of a 4-interval
    /// "tone in quiet" assessment, whose StimulusCollection produces an empty
    /// (ChannelSamples == 0) stream that the Windower transform then tried to window.
    /// </summary>
    public class StreamWindowerTests
    {
        private const int SkirtSamples = 441; // 10 ms @ 44.1 kHz, the PART default

        // An empty (0-sample) stream is the exact PART repro: it must window without
        // throwing and remain a 0-length stream.
        [Test]
        public void WindowingEmptyStreamDoesNotThrow()
        {
            IBGCStream stream = new SilenceStream(1, 0);

            IBGCStream windowed = null;
            Assert.DoesNotThrow(() =>
            {
                windowed = stream.Window(Windowing.Function.Hamming, SkirtSamples);
            });

            Assert.AreEqual(0, windowed.ChannelSamples);

            float[] buffer = new float[8];
            Assert.AreEqual(0, windowed.Read(buffer, 0, buffer.Length));
        }

        // Streams shorter than the combined skirts must window without throwing and
        // produce only finite, attenuated (|x| <= source amplitude) samples.
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(99)]   // odd
        [TestCase(100)]  // even
        [TestCase(881)]  // one less than 2 * SkirtSamples
        public void WindowingShortStreamProducesFiniteSamples(int channelSamples)
        {
            IBGCStream stream = new SineWave(1.0, 1000.0)
                .Truncate(totalChannelSamples: channelSamples)
                .Window(Windowing.Function.Hamming, SkirtSamples);

            Assert.AreEqual(channelSamples, stream.ChannelSamples);

            float[] buffer = new float[channelSamples];
            int read = stream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(channelSamples, read);

            for (int i = 0; i < read; i++)
            {
                Assert.IsFalse(float.IsNaN(buffer[i]), $"NaN at sample {i}");
                Assert.IsFalse(float.IsInfinity(buffer[i]), $"Infinity at sample {i}");
                Assert.LessOrEqual(Math.Abs(buffer[i]), 1.0f + 1e-4f, $"Sample {i} not attenuated");
            }
        }

        // Streams long enough for the requested skirts are windowed unchanged
        // (no silent reshaping of ordinary stimuli).
        [Test]
        public void WindowingLongStreamKeepsRequestedSkirts()
        {
            const int channelSamples = 22050; // 500 ms, far longer than 2 * SkirtSamples

            IBGCStream stream = new SineWave(1.0, 1000.0)
                .Truncate(totalChannelSamples: channelSamples)
                .Window(Windowing.Function.Hamming, SkirtSamples);

            Assert.AreEqual(channelSamples, stream.ChannelSamples);

            float[] buffer = new float[channelSamples];
            int read = stream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(channelSamples, read);

            // The opening skirt ramps from ~0; the steady middle reaches full amplitude.
            float maxMagnitude = 0f;
            for (int i = 0; i < read; i++)
            {
                maxMagnitude = Math.Max(maxMagnitude, Math.Abs(buffer[i]));
            }

            Assert.Greater(maxMagnitude, 0.99f, "Full-amplitude region was unexpectedly attenuated");
        }

        // GetHalfWindow / GetHalfWindow64 should reject a negative count loudly rather
        // than surfacing a message-less OverflowException from new float[negative].
        [Test]
        public void GetHalfWindowRejectsNegativeSampleCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Windowing.GetHalfWindow(Windowing.Function.Hamming, -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Windowing.GetHalfWindow64(Windowing.Function.Hamming, -1));
        }

        [Test]
        public void GetHalfWindowAllowsZeroSampleCount()
        {
            Assert.AreEqual(0, Windowing.GetHalfWindow(Windowing.Function.Hamming, 0).Length);
            Assert.AreEqual(0, Windowing.GetHalfWindow64(Windowing.Function.Hamming, 0).Length);
        }
    }
}

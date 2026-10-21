using System;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Synthesis;

namespace BGC.Tests
{
    /// <summary>
    /// STMAudioClip's raw gain (it never divides out the inverse FFT's sqrt N) must not depend on
    /// the frame its onset fix chooses: a duration just under a power of two gets a doubled frame
    /// for its wrap margin and would otherwise come out 3.01 dB louder, which would change the
    /// level of an unregulated STM mixed with another stimulus under a collection-level regulator.
    /// </summary>
    public class STMAudioClipFrameTests
    {
        private static double RawLevelDB(double duration, int seed)
        {
            STMAudioClip clip = new STMAudioClip(duration, 400.0, 3200.0, 2000, 0.0, 2.0, 4.0,
                STMAudioClip.RippleDirection.Up, STMAudioClip.AmplitudeDistribution.Pink, new Random(seed));
            float[] samples = new float[clip.ChannelSamples];
            int read = 0;
            while (read < samples.Length)
            {
                int n = clip.Read(samples, read, samples.Length - read);
                if (n <= 0) break;
                read += n;
            }
            return 20.0 * Math.Log10(Math.Sqrt(samples.Average(x => x * (double)x)));
        }

        /// <summary>
        /// 1.30 s (57330 samples) keeps a 65536 frame; 1.40 s (61740) is within 1/8 of it and gets
        /// 131072. Same seed, same carriers (depth 0, so no modulation): the raw levels must agree to
        /// the stationary noise's sample variance, not differ by 3.01 dB.
        /// </summary>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void RawGain_IsIndependentOfTheFrameSize(int seed)
        {
            Assert.AreEqual(65536, SingleFrequencyDomainToneComposer.FrameSize((int)Math.Ceiling(1.30 * 44100.0)), "1.30 s frame");
            Assert.AreEqual(131072, SingleFrequencyDomainToneComposer.FrameSize((int)Math.Ceiling(1.40 * 44100.0)), "1.40 s frame");

            double shorter = RawLevelDB(1.30, seed);
            double longer = RawLevelDB(1.40, seed);
            TestContext.WriteLine($"seed {seed}: raw level 1.30 s {shorter:F3} dB, 1.40 s {longer:F3} dB, difference {longer - shorter:+0.000} dB");
            Assert.AreEqual(0.0, longer - shorter, 0.5, "The doubled frame changed the raw gain (3.01 dB would be the uncorrected sqrt 2)");
        }
    }
}

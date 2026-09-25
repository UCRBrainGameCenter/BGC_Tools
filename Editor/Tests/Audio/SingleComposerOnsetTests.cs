using System;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Synthesis;
using BGC.Mathematics;

namespace BGC.Tests
{
    /// <summary>
    /// <see cref="SingleFrequencyDomainToneComposer"/> output must not start with a transient.
    /// An off-bin carrier isn't periodic in the composer's power-of-two frame, and the truncated
    /// sideband series smears the jump at the frame's wrap-around into a transient. Reading the
    /// output from the start of the frame put that transient at the start of every stimulus:
    /// -0.2 to -0.5 dB over the first 50 ms of a 500 ms or 1 s buffer, and a first few ms that were
    /// barely the intended tone at all (up to -15 dB). The composer now reads from the middle of
    /// the frame's unused part, with each carrier rotated to keep its phase at the first sample.
    /// </summary>
    public class SingleComposerOnsetTests
    {
        private const double SampleRate = 44100.0;

        private static float[] Render(double duration, params ComplexCarrierTone[] carriers)
        {
            SingleFrequencyDomainToneComposer composer = new SingleFrequencyDomainToneComposer(carriers, duration);
            float[] samples = new float[composer.ChannelSamples];
            composer.Read(samples, 0, samples.Length);
            return samples;
        }

        private static double LevelDB(float[] x, int start, int count, double amplitude)
        {
            double sum = 0.0;
            for (int i = start; i < start + count; i++)
            {
                sum += x[i] * (double)x[i];
            }
            return 10.0 * Math.Log10(sum / count / (0.5 * amplitude * amplitude));
        }

        /// <summary> Off-bin tones in a 500 ms buffer (frame 32768) and a 1 s buffer (frame 65536) </summary>
        [TestCase(0.5, 0.25)]
        [TestCase(0.5, 0.5)]
        [TestCase(0.5, 0.75)]
        [TestCase(1.0, 0.25)]
        [TestCase(1.0, 0.5)]
        [TestCase(1.0, 0.75)]
        public void OffBinTone_FirstAndLast50ms_AtItsLevel(double duration, double binFraction)
        {
            int frame = ((int)Math.Ceiling(duration * SampleRate)).CeilingToPowerOfTwo();
            double frequency = (Math.Floor(1000.0 * frame / SampleRate) + binFraction) * SampleRate / frame;
            float[] x = Render(duration, new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(0.1, 1.0)));

            // Relative to the middle of the buffer, so the test doesn't depend on the output scale
            const int window = 2205;
            double middle = LevelDB(x, x.Length / 2 - window / 2, window, 0.1);
            double first = LevelDB(x, 0, window, 0.1) - middle;
            double last = LevelDB(x, x.Length - window, window, 0.1) - middle;

            Assert.AreEqual(0.0, first, 0.1, $"First 50 ms at {first:+0.000;-0.000} dB re the middle");
            Assert.AreEqual(0.0, last, 0.1, $"Last 50 ms at {last:+0.000;-0.000} dB re the middle");
        }

        /// <summary>
        /// On-bin carriers are periodic in the frame, so the read offset must leave them exactly as
        /// specified: amplitude and phase at the first sample.
        /// </summary>
        [TestCase(0.0)]
        [TestCase(1.0)]
        [TestCase(2.5)]
        public void OnBinTone_SamplesAreTheSpecifiedTone(double phase)
        {
            const double duration = 0.5;
            const int frame = 32768;
            double frequency = 1486.0 * SampleRate / frame;
            float[] x = Render(duration, new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(0.1, phase)));

            double[] reference = Enumerable.Range(0, x.Length)
                .Select(i => Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase)).ToArray();

            // Least-squares amplitude, so the test checks shape and phase, not the output scale
            double scale = Enumerable.Range(0, x.Length).Sum(i => x[i] * reference[i]) /
                reference.Sum(r => r * r);
            double worst = Enumerable.Range(0, x.Length).Max(i => Math.Abs(x[i] - scale * reference[i])) / scale;

            Assert.Greater(scale, 0.0, "The tone came out inverted");
            Assert.Less(worst, 1e-4, $"Largest deviation from the specified tone: {worst:E2} of its amplitude");
        }
    }
}

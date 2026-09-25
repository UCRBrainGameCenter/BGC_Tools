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

            // Against the carrier's own level (A / sqrt 2), so a scale error fails too
            const int window = 2205;
            double middle = LevelDB(x, x.Length / 2 - window / 2, window, 0.1);
            double first = LevelDB(x, 0, window, 0.1);
            double last = LevelDB(x, x.Length - window, window, 0.1);

            Assert.AreEqual(0.0, middle, 0.01, $"Middle 50 ms at {middle:+0.000;-0.000} dB re the carrier");
            Assert.AreEqual(0.0, first, 0.1, $"First 50 ms at {first:+0.000;-0.000} dB re the carrier");
            Assert.AreEqual(0.0, last, 0.1, $"Last 50 ms at {last:+0.000;-0.000} dB re the carrier");
        }

        /// <summary>
        /// Off-bin tones over the first and last 20 ms, and sample by sample. Away from the wrap the
        /// untapered series still left a ripple decaying only as 1 / distance: +0.2 dB over the first
        /// and last 20 ms of a 1 s tone, and up to 3% of the amplitude in a single sample. The
        /// Hann-tapered synthesis leaves about 0.001 dB and 1.4e-4 of the amplitude.
        /// </summary>
        [TestCase(0.5, 0.5)]
        [TestCase(1.0, 0.25)]
        [TestCase(1.0, 0.5)]
        [TestCase(2.0, 0.5)]
        public void OffBinTone_First20msAndEverySample_AtTheTone(double duration, double binFraction)
        {
            int frame = ((int)Math.Ceiling(duration * SampleRate)).CeilingToPowerOfTwo();
            double frequency = (Math.Floor(1000.0 * frame / SampleRate) + binFraction) * SampleRate / frame;
            const double amplitude = 0.1;
            const double phase = 0.3;
            float[] x = Render(duration, new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(amplitude, phase)));

            const int window = 882;
            double first = LevelDB(x, 0, window, amplitude);
            double last = LevelDB(x, x.Length - window, window, amplitude);
            double worst = Enumerable.Range(0, x.Length)
                .Max(i => Math.Abs(x[i] - amplitude * Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase))) / amplitude;

            Assert.AreEqual(0.0, first, 0.01, $"First 20 ms at {first:+0.0000;-0.0000} dB");
            Assert.AreEqual(0.0, last, 0.01, $"Last 20 ms at {last:+0.0000;-0.0000} dB");
            Assert.Less(worst, 1e-3, $"Largest deviation from the tone: {worst:E2} of its amplitude");
        }

        /// <summary>
        /// Durations that nearly fill their power-of-two frame, measured against the ideal tone at its
        /// absolute amplitude. With the output next to the frame's wrap (independent review, report 14:
        /// 63448, 64000 and 65536 samples in a 65536 frame) the first 20 ms were -0.12, +0.69 and
        /// -4.75 dB. <see cref="SingleFrequencyDomainToneComposer.FrameSize"/> now leaves a margin.
        /// </summary>
        [TestCase(63448, 0.5)]
        [TestCase(64000, 0.5)]
        [TestCase(65536, 0.5)]
        [TestCase(65536, 0.25)]
        [TestCase(32700, 0.5)]
        [TestCase(131000, 0.75)]
        public void OffBinTone_NearlyFillingItsFrame_First20msAndEverySample_AtTheTone(int sampleCount, double binFraction)
        {
            // Off-bin relative to the frame the composer actually uses
            int frame = SingleFrequencyDomainToneComposer.FrameSize(sampleCount);
            double frequency = (Math.Floor(1000.0 * frame / SampleRate) + binFraction) * SampleRate / frame;
            const double amplitude = 0.1;
            const double phase = 0.3;

            SingleFrequencyDomainToneComposer composer = new SingleFrequencyDomainToneComposer(
                new[] { new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(amplitude, phase)) }, sampleCount);
            float[] x = new float[composer.ChannelSamples];
            composer.Read(x, 0, x.Length);

            const int window = 882;
            double first = LevelDB(x, 0, window, amplitude);
            double last = LevelDB(x, x.Length - window, window, amplitude);
            double worst = Enumerable.Range(0, x.Length)
                .Max(i => Math.Abs(x[i] - amplitude * Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase))) / amplitude;

            Assert.AreEqual(0.0, first, 0.01, $"{sampleCount} samples (frame {frame}): first 20 ms at {first:+0.0000;-0.0000} dB");
            Assert.AreEqual(0.0, last, 0.01, $"{sampleCount} samples (frame {frame}): last 20 ms at {last:+0.0000;-0.0000} dB");
            Assert.Less(worst, 1e-3, $"Largest deviation from the tone: {worst:E2} of its amplitude");
        }

        /// <summary>
        /// The claim must count exactly the carriers the frame renders. A 0.5 Hz carrier is below
        /// the first bin of a 65536 frame (0.67 Hz) but renderable in the 131072 frame a
        /// 65536-sample buffer now uses; the claim has to use the same frame.
        /// </summary>
        [Test]
        public void Claim_UsesTheFrameTheSamplesAreRenderedIn()
        {
            const int sampleCount = 65536;
            ComplexCarrierTone low = new ComplexCarrierTone(0.5, new Complex64(0.1, 0.0));
            SingleFrequencyDomainToneComposer composer = new SingleFrequencyDomainToneComposer(new[] { low }, sampleCount);

            float[] x = new float[sampleCount];
            composer.Read(x, 0, x.Length);
            double sampleRMS = Math.Sqrt(x.Sum(v => v * (double)v) / x.Length);
            bool renderable = FrequencyDomain.IsRenderable(SingleFrequencyDomainToneComposer.FrameSize(sampleCount), low.frequency);

            Assert.IsTrue(renderable, "Precondition: 0.5 Hz is renderable in the composer's frame");
            Assert.Greater(sampleRMS, 0.01, "The carrier was not rendered");
            Assert.AreEqual(0.1 / Math.Sqrt(2.0), composer.GetChannelRMS().First(), 1e-12, "The rendered carrier was not claimed");
        }

        /// <summary>
        /// Carriers at the edge bins (the first bin and the last below Nyquist) of a 1 s buffer,
        /// whose taper terms fall outside [1, N/2]: still exactly the sampled tone. (A carrier on the
        /// Nyquist bin itself isn't rendered: see <see cref="FrequencyDomain.IsRenderable"/>.)
        /// </summary>
        [TestCase(1, 0.7)]
        [TestCase(32767, 0.0)]
        [TestCase(32767, 1.0)]
        public void EdgeBinTone_SamplesAreTheSampledTone(int bin, double phase)
        {
            const double duration = 1.0;
            const int frame = 65536;
            const double amplitude = 0.1;
            double frequency = bin * SampleRate / frame;
            float[] x = Render(duration, new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(amplitude, phase)));

            double worst = Enumerable.Range(0, x.Length)
                .Max(i => Math.Abs(x[i] - amplitude * Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase))) / amplitude;

            Assert.Less(worst, 1e-4, $"Largest deviation from the sampled tone: {worst:E2} of its amplitude");
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

            // Least-squares amplitude: shape and phase against the fitted scale, and the scale itself
            // against the specified amplitude (0.1)
            double scale = Enumerable.Range(0, x.Length).Sum(i => x[i] * reference[i]) /
                reference.Sum(r => r * r);
            double worst = Enumerable.Range(0, x.Length).Max(i => Math.Abs(x[i] - scale * reference[i])) / scale;

            Assert.Greater(scale, 0.0, "The tone came out inverted");
            Assert.AreEqual(0.1, scale, 1e-5, "The tone's amplitude is not the specified one (output scale)");
            Assert.Less(worst, 1e-4, $"Largest deviation from the specified tone: {worst:E2} of its amplitude");
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Synthesis;
using BGC.Mathematics;

namespace BGC.Tests
{
    /// <summary>
    /// A carrier whose frequency falls between FFT bins must be synthesized at its frequency and
    /// with its phase. <see cref="FrequencyDomain.Populate(Complex64[], double, Complex64, int)"/>
    /// used half the fractional bin offset (tones landed up to half a bin low, e.g. -0.34 Hz at
    /// half a bin in a 500 ms buffer) and the opposite sign in its sideband terms (off-bin tones
    /// were inverted relative to on-bin ones).
    ///
    /// Measured away from the frame edges, where the truncated sideband series converges.
    /// Correlation and frequency are independent of the composer's output scale.
    /// </summary>
    public class FrequencyDomainOffBinTests
    {
        private const double SampleRate = 44100.0;
        private const int FrameSize = 32768;
        private const double Duration = 0.5;

        private static double[] Render(double frequency, double phase)
        {
            SingleFrequencyDomainToneComposer composer = new SingleFrequencyDomainToneComposer(
                carrierTones: new[] { new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(0.1, phase)) },
                duration: Duration);

            float[] samples = new float[composer.ChannelSamples];
            composer.Read(samples, 0, samples.Length);
            return samples.Select(x => (double)x).ToArray();
        }

        private static double Correlation(double[] x, double frequency, double phase, int start, int count)
        {
            double sxy = 0, sxx = 0, syy = 0;
            for (int i = start; i < start + count; i++)
            {
                double y = Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase);
                sxy += x[i] * y;
                sxx += x[i] * x[i];
                syy += y * y;
            }
            return sxy / Math.Sqrt(sxx * syy);
        }

        /// <summary> Frequency of the largest projection within +/-1 Hz, in 0.002 Hz steps </summary>
        private static double EstimateFrequency(double[] x, double nominal, int start, int count)
        {
            double best = 0.0, bestFrequency = nominal;
            for (double f = nominal - 1.0; f <= nominal + 1.0; f += 0.002)
            {
                double re = 0, im = 0;
                for (int i = start; i < start + count; i++)
                {
                    double w = 2.0 * Math.PI * f * i / SampleRate;
                    re += x[i] * Math.Cos(w);
                    im += x[i] * Math.Sin(w);
                }
                double power = re * re + im * im;
                if (power > best)
                {
                    best = power;
                    bestFrequency = f;
                }
            }
            return bestFrequency;
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.25, 0.0)]
        [TestCase(0.5, 0.0)]
        [TestCase(0.5, 1.0)]
        [TestCase(0.75, 2.0)]
        [TestCase(0.9, 0.5)]
        public void OffBinTone_HasItsFrequencyAndPhase(double binFraction, double phase)
        {
            double frequency = (1486.0 + binFraction) * SampleRate / FrameSize;
            double[] x = Render(frequency, phase);

            // Middle of the 22050-sample buffer, clear of the frame-edge transient
            const int start = 5000, count = 12000;

            double estimated = EstimateFrequency(x, frequency, start, count);
            double correlation = Correlation(x, frequency, phase, start, count);

            Assert.AreEqual(frequency, estimated, 0.01,
                $"Bin + {binFraction}: synthesized at {estimated:F3} Hz, intended {frequency:F3} Hz");
            Assert.Greater(correlation, 0.999,
                $"Bin + {binFraction}, phase {phase}: correlation with the intended tone {correlation:F4}");
        }

        /// <summary>
        /// The Continuous composer advances each frame's carrier phase at the carrier's true
        /// frequency, so frames only overlap-add coherently if Populate synthesizes that frequency.
        /// With the half-offset bug its level swung by about 1.3 dB with the bin fraction.
        /// Its level must not depend on where the tone falls between bins.
        /// </summary>
        [Test]
        public void ContinuousComposer_LevelDoesNotDependOnBinFraction()
        {
            const int frame = 2048;
            double[] levels = new[] { 0.0, 0.25, 0.5, 0.75 }.Select(binFraction =>
            {
                double frequency = (46.0 + binFraction) * SampleRate / frame;
                ContinuousFrequencyDomainToneComposer composer = new ContinuousFrequencyDomainToneComposer(
                    new[] { new ComplexCarrierTone(frequency, new Complex64(0.1, 0.0)) });

                float[] samples = new float[2 * frame + (int)SampleRate];
                composer.Read(samples, 0, samples.Length);

                double sum = 0.0;
                for (int i = 2 * frame; i < samples.Length; i++)
                {
                    sum += samples[i] * (double)samples[i];
                }
                return 10.0 * Math.Log10(sum / SampleRate);
            }).ToArray();

            Assert.Less(levels.Max() - levels.Min(), 0.05,
                $"Levels across bin fractions: {string.Join(", ", levels.Select(x => x.ToString("F3")))} dB");
        }

        /// <summary>
        /// The untapered <see cref="FrequencyDomain.Populate(Complex64[], double, Complex64, int)"/>
        /// itself, which FD(Continuous), STM and the noise clips use (FD(Single) now uses the
        /// tapered variant, so the tests above can't see a regression confined to Populate). One
        /// frame, inverse FFT, scaled by 1 / sqrt(N): mid-frame, clear of the wrap, the samples must
        /// be A cos(2 pi f n / fs + phi) at the carrier's frequency, phase and amplitude. A sideband
        /// sign error inverts off-bin tones; a halved offset moves them.
        /// </summary>
        [TestCase(0.0, 0.0)]
        [TestCase(0.25, 0.0)]
        [TestCase(0.5, 1.0)]
        [TestCase(0.75, 2.0)]
        public void UntaperedPopulate_OffBinTone_HasItsFrequencyPhaseAndAmplitude(double binFraction, double phase)
        {
            const double amplitude = 0.1;
            double frequency = (1486.0 + binFraction) * SampleRate / FrameSize;
            Complex64[] buffer = new Complex64[FrameSize];
            FrequencyDomain.Populate(buffer, frequency, Complex64.FromPolarCoordinates(amplitude, phase));
            Fourier.Inverse(buffer);

            const int start = FrameSize / 4, count = FrameSize / 2;
            double worst = 0.0;
            for (int i = start; i < start + count; i++)
            {
                double sample = buffer[i].Real / Math.Sqrt(FrameSize);
                worst = Math.Max(worst, Math.Abs(sample - amplitude * Math.Cos(2.0 * Math.PI * frequency * i / SampleRate + phase)));
            }

            // The truncated (+/-20-term) untapered series ripples by up to ~3% of the amplitude per
            // sample (2.1% measured mid-frame at bin + 0.5); a sign error is ~200%, and a halved
            // offset (d / 2 bins low) drifts the phase by pi d / 2 across this half frame
            Assert.Less(worst / amplitude, 0.03,
                $"Bin + {binFraction}, phase {phase}: mid-frame samples deviate from the tone by up to {worst / amplitude:E2} of its amplitude");
        }

        /// <summary>
        /// FD(Continuous) keeps each carrier's phase (its output sample n is time n / fs), not only
        /// its level: correlation with the intended tone after the overlap-add build-up, on and off
        /// bin. (Its level is +0.66 dB, the known window gain; correlation ignores scale.)
        /// </summary>
        [TestCase(0.0, 0.0)]
        [TestCase(0.25, 1.0)]
        [TestCase(0.5, 2.0)]
        [TestCase(0.75, -1.0)]
        public void ContinuousComposer_OffBinTone_HasItsPhase(double binFraction, double phase)
        {
            const int frame = 2048;
            double frequency = (46.0 + binFraction) * SampleRate / frame;
            ContinuousFrequencyDomainToneComposer composer = new ContinuousFrequencyDomainToneComposer(
                new[] { new ComplexCarrierTone(frequency, Complex64.FromPolarCoordinates(0.1, phase)) });

            double[] x = new double[2 * frame + (int)SampleRate];
            float[] samples = new float[x.Length];
            composer.Read(samples, 0, samples.Length);
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = samples[i];
            }

            double correlation = Correlation(x, frequency, phase, 2 * frame, (int)SampleRate);
            Assert.Greater(correlation, 0.999,
                $"Bin + {binFraction}, phase {phase}: correlation with the intended tone {correlation:F4}");
        }
    }
}

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
    }
}

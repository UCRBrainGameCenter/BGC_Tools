using System;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Mathematics.FIRFilters;

namespace BGC.Tests
{
    /// <summary>
    /// <see cref="ConvolutionFilter"/> (overlap-add FFT convolution) must equal the direct,
    /// time-domain convolution y[n] = sum_k h[k] x[n - k], sample for sample. The spectrum of the
    /// filter must be compensated for both forward transforms' 1/N scaling across every bin, as
    /// <see cref="MultiConvolutionFilter"/> does.
    /// </summary>
    public class ConvolutionFilterTests
    {
        private static float[] RandomSignal(int length, int seed)
        {
            Random random = new Random(seed);
            return Enumerable.Range(0, length).Select(_ => (float)(2.0 * random.NextDouble() - 1.0)).ToArray();
        }

        private static double[] DirectConvolution(float[] x, double[] h)
        {
            double[] y = new double[x.Length + h.Length - 1];
            for (int n = 0; n < y.Length; n++)
            {
                double sum = 0.0;
                for (int k = Math.Max(0, n - x.Length + 1); k <= Math.Min(n, h.Length - 1); k++)
                {
                    sum += h[k] * x[n - k];
                }
                y[n] = sum;
            }
            return y;
        }

        private static float[] ReadAll(IBGCStream stream)
        {
            Assert.AreNotEqual(int.MaxValue, stream.ChannelSamples);
            float[] samples = new float[stream.TotalSamples];
            int read = stream.Read(samples, 0, samples.Length);
            Assert.AreEqual(samples.Length, read, "Stream ended early");
            return samples;
        }

        private static void AssertMatchesDirect(float[] x, double[] h)
        {
            double[] expected = DirectConvolution(x, h);
            float[] actual = ReadAll(new SimpleAudioClip(x, channels: 1).Convolve(h));

            Assert.AreEqual(expected.Length, actual.Length, "Output length");

            double peak = expected.Max(v => Math.Abs(v));
            double worst = 0.0;
            int worstIndex = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                double error = Math.Abs(actual[i] - expected[i]);
                if (error > worst)
                {
                    worst = error;
                    worstIndex = i;
                }
            }

            Assert.LessOrEqual(worst, 1e-4 * peak,
                $"Largest error {worst:E3} at sample {worstIndex} (expected {expected[worstIndex]:F6}, got {actual[worstIndex]:F6}; peak {peak:F6})");
        }

        [Test]
        public void UnitImpulse_IsIdentity()
        {
            AssertMatchesDirect(RandomSignal(5000, 1), new double[] { 1.0 });
        }

        [TestCase(31)]
        [TestCase(100)]
        [TestCase(300)]
        public void ArbitraryFilter_MatchesDirectConvolution(int filterLength)
        {
            Random random = new Random(filterLength);
            double[] h = Enumerable.Range(0, filterLength).Select(_ => 2.0 * random.NextDouble() - 1.0).ToArray();
            AssertMatchesDirect(RandomSignal(5000, 2), h);
        }

        /// <summary> The filter of Noise-GINOzLevitt's "Butterworth FIR Bandpass" (order 510, 100-1000 Hz) </summary>
        [Test]
        public void GINOzButterworth_MatchesDirectConvolution()
        {
            double[] h = FIRButterworth.BandpassCoefficients(order: 510, f1: 100, f2: 1000, samplingRate: 44100f);
            AssertMatchesDirect(RandomSignal(10000, 3), h);
        }
    }
}

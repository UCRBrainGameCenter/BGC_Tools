using System;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;

namespace BGC.Tests
{
    /// <summary>
    /// An overlap-add convolution of an N-sample input with an L-tap filter has N + L - 1 samples.
    /// <see cref="BGC.Audio.Filters.ConvolutionFilter"/> and
    /// <see cref="BGC.Audio.Filters.MultiConvolutionFilter"/> only emitted the filter's tail
    /// (the last L - 1 samples) when the final input block was partial. When the input length was
    /// an exact multiple of the block size (fftLength - L), they stopped after N samples while still
    /// reporting N + L - 1, and a Recalculate RMS divided the shorter output by the longer length.
    /// </summary>
    public class ConvolutionTailTests
    {
        private static float[] RandomSignal(int length, int seed)
        {
            Random random = new Random(seed);
            return Enumerable.Range(0, length).Select(_ => (float)(2.0 * random.NextDouble() - 1.0)).ToArray();
        }

        private static double[] RandomFilter(int length, int seed)
        {
            Random random = new Random(seed);
            return Enumerable.Range(0, length).Select(_ => 2.0 * random.NextDouble() - 1.0).ToArray();
        }

        /// <summary> Read everything the stream will give, in small chunks </summary>
        private static float[] ReadToEnd(IBGCStream stream)
        {
            float[] output = new float[stream.TotalSamples + 4096];
            int total = 0;
            int read;
            while ((read = stream.Read(output, total, Math.Min(700, output.Length - total))) > 0)
            {
                total += read;
            }
            return output.Take(total).ToArray();
        }

        /// <summary>
        /// Block size (fftLength - L): 513 for a 511-tap filter (fftLength 1024), 481 for 31 taps
        /// (fftLength 512). Exact multiples of it, and a partial last block as a control.
        /// </summary>
        [TestCase(511, 513)]
        [TestCase(511, 1026)]
        [TestCase(511, 5130)]
        [TestCase(31, 481)]
        [TestCase(31, 4810)]
        [TestCase(511, 514)]
        public void ConvolutionFilter_EmitsItsWholeOutput(int filterLength, int inputLength)
        {
            IBGCStream convolved = new SimpleAudioClip(RandomSignal(inputLength, 1), channels: 1)
                .Convolve(RandomFilter(filterLength, 2));

            Assert.AreEqual(inputLength + filterLength - 1, convolved.ChannelSamples, "Reported length");

            float[] output = ReadToEnd(convolved);
            Assert.AreEqual(convolved.TotalSamples, output.Length, "Samples actually emitted");

            double tailEnergy = output.Skip(inputLength).Sum(x => x * (double)x);
            Assert.Greater(tailEnergy, 0.0, "The filter's tail is silent");
        }

        [TestCase(511, 513)]
        [TestCase(511, 1026)]
        [TestCase(511, 514)]
        public void MultiConvolutionFilter_MatchesDirectConvolutionToTheEnd(int filterLength, int inputLength)
        {
            float[] x = RandomSignal(inputLength, 3);
            double[] left = RandomFilter(filterLength, 4);
            double[] right = RandomFilter(filterLength, 5);

            IBGCStream convolved = new SimpleAudioClip(x, channels: 1).MultiConvolve(left, right);
            float[] output = ReadToEnd(convolved);

            Assert.AreEqual(2 * (inputLength + filterLength - 1), output.Length, "Samples actually emitted");

            for (int channel = 0; channel < 2; channel++)
            {
                double[] h = channel == 0 ? left : right;
                double worst = 0.0, peak = 0.0;
                for (int n = 0; n < inputLength + filterLength - 1; n++)
                {
                    double expected = 0.0;
                    for (int k = Math.Max(0, n - inputLength + 1); k <= Math.Min(n, h.Length - 1); k++)
                    {
                        expected += h[k] * x[n - k];
                    }
                    peak = Math.Max(peak, Math.Abs(expected));
                    worst = Math.Max(worst, Math.Abs(output[2 * n + channel] - expected));
                }

                Assert.LessOrEqual(worst, 1e-4 * peak, $"Channel {channel}: largest error {worst:E3} (peak {peak:F4})");
            }
        }
    }
}

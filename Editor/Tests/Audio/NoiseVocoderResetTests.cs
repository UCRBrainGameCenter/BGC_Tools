using System;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;

namespace BGC.Tests
{
    /// <summary>
    /// A Noise Vocoder plays the same samples on every read from the start: a level regulator
    /// measures the stream (a read, then Reset) before it plays it, so the measured realization
    /// must be the played one. Reset restored the noise that seeds the first frame but kept the
    /// last step of the previous read's noise buffer, which slides into the first frame's window:
    /// a fresh read and a read after Reset differed in their first frames.
    /// </summary>
    public class NoiseVocoderResetTests
    {
        private static float[] ReadAll(IBGCStream stream)
        {
            float[] samples = new float[stream.TotalSamples];
            int read = 0;
            while (read < samples.Length)
            {
                int n = stream.Read(samples, read, samples.Length - read);
                if (n <= 0)
                {
                    break;
                }
                read += n;
            }
            return samples;
        }

        [TestCase(4)]
        [TestCase(2)]
        public void ReadAfterReset_IsTheFreshRead(int overlapRatio)
        {
            IBGCStream vocoder = new NoiseVocoder(
                stream: new SineWave(0.1, 1000.0).Truncate(totalDuration: 0.5),
                freqLowerBound: 100.0,
                freqUpperBound: 8000.0,
                bandCount: 16,
                fftSize: 4096,
                overlapRatio: overlapRatio,
                randomizer: new Random(7));

            float[] fresh = ReadAll(vocoder);
            vocoder.Reset();
            float[] afterReset = ReadAll(vocoder);

            double difference = 0.0;
            for (int i = 0; i < fresh.Length; i++)
            {
                difference = Math.Max(difference, Math.Abs(fresh[i] - afterReset[i]));
            }

            Assert.AreEqual(0.0, difference, 0.0, "The read after Reset differs from the fresh read");
        }
    }
}

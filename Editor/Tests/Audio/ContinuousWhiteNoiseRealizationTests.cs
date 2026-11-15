using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;

namespace BGC.Tests
{
    /// <summary>
    /// ContinuousWhiteNoiseStream is one realization of noise: Reset and Seek return to its
    /// samples, as every other generator's do, so a level regulator that measures a stream (a
    /// read, then Reset) measures the samples that play. A loop (StreamRepeater) or a host
    /// replaying a sound asks for a new realization explicitly (NextRealization), so a looped or
    /// replayed noise is fresh noise each time, as it was when Reset didn't rewind.
    /// </summary>
    public class ContinuousWhiteNoiseRealizationTests
    {
        private const double Duration = 0.02;

        private static float[] ReadAll(IBGCStream stream, int samples)
        {
            float[] data = new float[samples];
            int read = 0;
            while (read < samples)
            {
                int n = stream.Read(data, read, samples - read);
                if (n <= 0)
                {
                    break;
                }
                read += n;
            }
            return data;
        }

        private static int Frames(IBGCStream stream) => (int)Math.Round(Duration * stream.SamplingRate);

        [Test]
        public void ReadAfterReset_IsTheSameNoise()
        {
            IBGCStream noise = new ContinuousWhiteNoiseStream(new Random(4)).Truncate(totalDuration: Duration);
            int frames = Frames(noise);

            float[] first = ReadAll(noise, frames);
            noise.Reset();
            float[] second = ReadAll(noise, frames);

            CollectionAssert.AreEqual(first, second, "A read after Reset is different noise");
        }

        [TestCase(1)]
        [TestCase(300)]
        public void Seek_LandsOnTheSameSamples(int position)
        {
            IBGCStream noise = new ContinuousWhiteNoiseStream(new Random(4));
            float[] first = ReadAll(noise, 1000);
            noise.Seek(position);
            float[] fromSeek = ReadAll(noise, 1000 - position);

            CollectionAssert.AreEqual(first.Skip(position).ToArray(), fromSeek, "Seek didn't land on the noise's samples");
        }

        [Test]
        public void Loop_PlaysFreshNoiseEachPass()
        {
            IBGCStream period = new ContinuousWhiteNoiseStream(new Random(4)).Truncate(totalDuration: Duration);
            int frames = Frames(period);
            float[] looped = ReadAll(period.Loop(), 3 * frames);

            float[][] passes = Enumerable.Range(0, 3).Select(p => looped.Skip(p * frames).Take(frames).ToArray()).ToArray();
            Assert.IsFalse(passes[0].SequenceEqual(passes[1]), "The second pass repeats the first");
            Assert.IsFalse(passes[1].SequenceEqual(passes[2]), "The third pass repeats the second");
        }

        [Test]
        public void NextRealization_IsFreshNoise_AndResetReturnsToIt()
        {
            IBGCStream noise = new ContinuousWhiteNoiseStream(new Random(4)).Truncate(totalDuration: Duration);
            int frames = Frames(noise);

            float[] first = ReadAll(noise, frames);
            noise.NextRealization();
            float[] next = ReadAll(noise, frames);
            noise.Reset();
            float[] nextAgain = ReadAll(noise, frames);

            Assert.IsFalse(first.SequenceEqual(next), "NextRealization replayed the same noise");
            CollectionAssert.AreEqual(next, nextAgain, "Reset after NextRealization didn't return to the new noise");
        }

        [Test]
        public void SameSeed_SameNoise()
        {
            IBGCStream a = new ContinuousWhiteNoiseStream(new Random(11));
            IBGCStream b = new ContinuousWhiteNoiseStream(new Random(11));
            CollectionAssert.AreEqual(ReadAll(a, 2000), ReadAll(b, 2000));
        }

        /// <summary>
        /// The clamp bounds the noise that plays. A 20 ms noise requested at the limit was measured
        /// on one realization and played the next, so a realization louder than the measured one
        /// played above the limit: 50 seeds, each must play at most the limit + 0.1 dB.
        /// </summary>
        [Test]
        public void Clamp_BoundsTheNoiseThatPlays()
        {
            float[] reference = Reference(Normalization.dbSafetyLimit);
            double limitDB = RmsDB(reference) + Normalization.deliveredLevelTolerance;
            List<double> excess = new List<double>();
            Action<SafetyLimitEvent> saved = SafetyLimit.EventReporter;
            SafetyLimit.EventReporter = _ => { };
            try
            {
                for (int seed = 0; seed < 50; seed++)
                {
                    IBGCStream noise = new ContinuousWhiteNoiseStream(new Random(seed)).Truncate(totalDuration: Duration);
                    IBGCStream played = noise.Normalize(Normalization.dbSafetyLimit, SafetyLimitMode.Clamp);
                    excess.Add(RmsDB(ReadAll(played, played.TotalSamples)) - limitDB);
                }
            }
            finally
            {
                SafetyLimit.EventReporter = saved;
            }

            TestContext.WriteLine($"Played level re limit + 0.1 dB, 50 seeds: max {excess.Max():+0.000;-0.000} dB");
            Assert.LessOrEqual(excess.Max(), 1e-3, "A clamped noise played above the limit");
        }

        private static float[] Reference(double level) =>
            ReadAll(new SineWave(0.1, 1000.0).Truncate(totalDuration: 0.5).Normalize(level, SafetyLimitMode.Off), (int)(2 * 0.5 * 44100));

        private static double RmsDB(float[] samples) => 20.0 * Math.Log10(Math.Sqrt(samples.Average(x => x * (double)x)));
    }
}

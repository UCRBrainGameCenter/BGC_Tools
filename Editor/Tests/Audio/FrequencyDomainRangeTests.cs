using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Synthesis;
using BGC.Mathematics;

namespace BGC.Tests
{
    /// <summary>
    /// <see cref="FrequencyDomain.Populate(Complex64[], double, Complex64, int)"/> skips carriers
    /// below the first FFT bin or above the Nyquist bin. The frequency-domain composers' claimed
    /// (Passthrough) RMS must skip the same carriers, or a Level Regulator scales the stimulus by a
    /// power it doesn't contain. Adding an unrenderable carrier must change neither the samples
    /// nor the claim.
    /// </summary>
    public class FrequencyDomainRangeTests
    {
        private static readonly ComplexCarrierTone InRange = new ComplexCarrierTone(1000.0, new Complex64(0.1, 0.0));

        private static float[] ReadAll(IBGCStream stream, int count)
        {
            float[] samples = new float[count];
            stream.Reset();
            stream.Read(samples, 0, count);
            return samples;
        }

        [TestCase(30_000.0)]   // above Nyquist
        [TestCase(0.1)]        // below the first bin of a 32768-sample frame (1.35 Hz)
        [TestCase(-500.0)]
        public void SingleComposer_UnrenderableCarrier_IsNotClaimed(double frequency)
        {
            ComplexCarrierTone outOfRange = new ComplexCarrierTone(frequency, new Complex64(0.1, 0.0));

            SingleFrequencyDomainToneComposer alone = new SingleFrequencyDomainToneComposer(new[] { InRange }, 0.5);
            SingleFrequencyDomainToneComposer withExtra = new SingleFrequencyDomainToneComposer(new[] { InRange, outOfRange }, 0.5);

            Assert.IsFalse(FrequencyDomain.IsRenderable(alone.ChannelSamples.CeilingToPowerOfTwo(), frequency));
            CollectionAssert.AreEqual(ReadAll(alone, alone.ChannelSamples), ReadAll(withExtra, withExtra.ChannelSamples),
                "The unrenderable carrier changed the samples");
            Assert.AreEqual(alone.GetChannelRMS().First(), withExtra.GetChannelRMS().First(), 1e-12,
                "The unrenderable carrier was counted in the claimed RMS");
        }

        [Test]
        public void ContinuousComposer_UnrenderableCarrier_IsNotClaimed()
        {
            // 10 Hz is below the first bin of the default 2048-sample frame (21.5 Hz)
            ComplexCarrierTone outOfRange = new ComplexCarrierTone(10.0, new Complex64(0.1, 0.0));

            ContinuousFrequencyDomainToneComposer alone = new ContinuousFrequencyDomainToneComposer(new[] { InRange });
            ContinuousFrequencyDomainToneComposer withExtra = new ContinuousFrequencyDomainToneComposer(new[] { InRange, outOfRange });

            Assert.IsFalse(FrequencyDomain.IsRenderable(1 << 11, 10.0));
            CollectionAssert.AreEqual(ReadAll(alone, 8192), ReadAll(withExtra, 8192),
                "The unrenderable carrier changed the samples");
            Assert.AreEqual(alone.GetChannelRMS().First(), withExtra.GetChannelRMS().First(), 1e-12,
                "The unrenderable carrier was counted in the claimed RMS");
        }

        [TestCase(2.0, true)]       // bin 1 of a 32768-sample frame
        [TestCase(22050.0, true)]   // the Nyquist bin itself is rendered
        [TestCase(22060.0, false)]
        [TestCase(0.5, false)]
        public void IsRenderable_MatchesPopulate(double frequency, bool renderable)
        {
            const int bufferSize = 1 << 15;
            Complex64[] buffer = new Complex64[bufferSize];
            FrequencyDomain.Populate(buffer, frequency, new Complex64(1.0, 0.0));

            Assert.AreEqual(renderable, FrequencyDomain.IsRenderable(bufferSize, frequency));
            Assert.AreEqual(renderable, buffer.Any(x => x.MagnitudeSquared > 0.0), "Populate disagrees");
        }
    }
}

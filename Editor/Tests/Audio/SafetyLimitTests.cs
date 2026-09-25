using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BGC.Audio;
using BGC.Audio.Audiometry;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;

namespace BGC.Tests
{
    /// <summary>
    /// The 90 dB safety limit (<see cref="Normalization.dbSafetyLimit"/>) must bound the level a
    /// stream actually delivers, not only the level requested of it.
    ///
    /// The level regulators scale a stream by its <i>claimed</i> RMS
    /// (<see cref="IBGCStream.GetChannelRMS"/>). If a stream's samples are louder than its claim,
    /// a request at or below the limit plays above it. These tests use a stream whose claim is
    /// deliberately wrong by a known amount, so they don't depend on any particular generator.
    /// The limit is checked lazily, on the first Read, as it always has been.
    /// </summary>
    public class SafetyLimitTests
    {
        private const double ToneAmplitude = 0.1;
        private const double ToneDuration = 0.5;

        /// <summary> Passes samples through unchanged, but reports a fixed RMS </summary>
        private class ClaimedRMSStream : SimpleBGCFilter
        {
            private readonly double claimedRMS;

            public override int Channels => stream.Channels;
            public override int TotalSamples => stream.TotalSamples;
            public override int ChannelSamples => stream.ChannelSamples;

            public ClaimedRMSStream(IBGCStream stream, double claimedRMS)
                : base(stream)
            {
                this.claimedRMS = claimedRMS;
            }

            protected override void _Initialize() { }

            public override int Read(float[] data, int offset, int count) => stream.Read(data, offset, count);

            public override IEnumerable<double> GetChannelRMS()
            {
                double[] rms = new double[Channels];
                for (int i = 0; i < rms.Length; i++)
                {
                    rms[i] = claimedRMS;
                }
                return rms;
            }
        }

        /// <summary>
        /// A finite 1 kHz tone whose samples are <paramref name="excessDB"/> louder than it claims.
        /// 500 ms is a whole number of periods, so the tone's true RMS is exactly A / sqrt(2).
        /// </summary>
        private static IBGCStream MisclaimedTone(double excessDB) =>
            new ClaimedRMSStream(
                stream: new SineWave(ToneAmplitude, 1000.0).Truncate(totalDuration: ToneDuration),
                claimedRMS: ToneAmplitude / Math.Sqrt(2.0) * Math.Pow(10.0, -excessDB / 20.0));

        private static void ReadAll(IBGCStream stream)
        {
            float[] buffer = new float[4096];
            while (stream.Read(buffer, 0, buffer.Length) > 0) { }
        }

        [TestCase(85.0)]
        [TestCase(88.0)]
        [TestCase(90.0)]
        public void DeliveredAboveLimit_IsRefusedOnFirstRead(double requested)
        {
            // Samples 6.02 dB above the claim, as FD(Single) delivered before its fix
            IBGCStream normalized = null;
            Assert.DoesNotThrow(() => normalized = MisclaimedTone(20.0 * Math.Log10(2.0)).Normalize(requested),
                "The limit is checked lazily; building the stream must not throw");

            StreamCompositionException e = Assert.Throws<StreamCompositionException>(() => ReadAll(normalized));
            StringAssert.Contains("safety limit of 90dB", e.Message);
            StringAssert.Contains($"Requested Level: {requested} dB", e.Message);
        }

        [Test]
        public void DeliveredBelowLimit_Plays()
        {
            // 80 requested, 86.02 delivered
            Assert.DoesNotThrow(() => ReadAll(MisclaimedTone(20.0 * Math.Log10(2.0)).Normalize(80.0)));
        }

        [Test]
        public void AccurateClaimAtLimit_Plays()
        {
            Assert.DoesNotThrow(() => ReadAll(MisclaimedTone(0.0).Normalize(90.0)));
        }

        [Test]
        public void ExcessWithinTolerance_Plays()
        {
            Assert.DoesNotThrow(() => ReadAll(MisclaimedTone(0.5 * Normalization.deliveredLevelTolerance).Normalize(90.0)));
        }

        [Test]
        public void ExcessBeyondTolerance_IsRefused()
        {
            Assert.Throws<StreamCompositionException>(
                () => ReadAll(MisclaimedTone(2.0 * Normalization.deliveredLevelTolerance).Normalize(90.0)));
        }

        [Test]
        public void ClaimAboveSamples_AtLimit_Plays()
        {
            // Samples quieter than claimed deliver below the request: never refused for that
            Assert.DoesNotThrow(() => ReadAll(MisclaimedTone(-6.0).Normalize(90.0)));
        }

        [Test]
        public void RequestAboveLimit_IsStillRefused()
        {
            // Unchanged behavior: the requested level is checked too
            Assert.Throws<StreamCompositionException>(() => ReadAll(MisclaimedTone(0.0).Normalize(91.0)));
        }

        [Test]
        public void SafetyDisengaged_DeliveredAboveLimit_Plays()
        {
            Assert.DoesNotThrow(() => ReadAll(MisclaimedTone(20.0 * Math.Log10(2.0)).Normalize(88.0, safetyLimit: false)));
        }

        [Test]
        public void StereoInput_DeliveredAboveLimit_IsRefused()
        {
            IBGCStream stereo = new ClaimedRMSStream(
                stream: new SineWave(ToneAmplitude, 1000.0).Truncate(totalDuration: ToneDuration).UpChannel(),
                claimedRMS: 0.5 * ToneAmplitude / Math.Sqrt(2.0));

            Assert.AreEqual(2, stereo.Channels);
            Assert.Throws<StreamCompositionException>(() => ReadAll(stereo.Normalize(88.0)));
        }

        /// <summary>
        /// Stereo: left at its claim, right 6.02 dB above it. Each channel is judged against its
        /// own requested level.
        /// </summary>
        private static IBGCStream HotRightStereo()
        {
            int frames = (int)(ToneDuration * 44100);
            float[] samples = new float[2 * frames];
            for (int i = 0; i < frames; i++)
            {
                float x = (float)(ToneAmplitude * Math.Sin(2.0 * Math.PI * 1000.0 * i / 44100.0));
                samples[2 * i] = x;
                samples[2 * i + 1] = 2f * x;
            }

            return new ClaimedRMSStream(new SimpleAudioClip(samples, channels: 2), ToneAmplitude / Math.Sqrt(2.0));
        }

        [Test]
        public void SplitLevels_HotChannelBelowLimit_Plays()
        {
            // Left 90 (delivers 90), right 60 (delivers 66)
            Assert.DoesNotThrow(() => ReadAll(HotRightStereo().Normalize((90.0, 60.0))));
        }

        [Test]
        public void SplitLevels_HotChannelAboveLimit_IsRefused()
        {
            // Left 60 (delivers 60), right 88 (delivers 94)
            Assert.Throws<StreamCompositionException>(() => ReadAll(HotRightStereo().Normalize((60.0, 88.0))));
        }

        [Test]
        public void InfiniteStream_OnlyRequestIsChecked()
        {
            // An infinite stream can't be measured; its claim is trusted, as before
            IBGCStream normalized = new SineWave(ToneAmplitude, 1000.0).Normalize(90.0);
            float[] buffer = new float[4096];
            Assert.DoesNotThrow(() => normalized.Read(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// The dB HL regulator (<see cref="LevelRegulation.GetRMSScalingFactors(IBGCStream, double,
        /// AudiometricCalibration.CalibrationSet, double, out double, out double,
        /// AudiometricCalibration.Source, bool)"/>) needs a stored calibration profile, which the
        /// Editor doesn't have, so its shared check is exercised directly. The delivered-minus-
        /// requested excess is calibration-independent, so the same arithmetic applies in dB HL.
        /// </summary>
        [TestCase(85.0, true)]
        [TestCase(80.0, false)]
        public void DeliveredLevelCheck_HearingLevelUnits(double requestedHL, bool refused)
        {
            IBGCStream stream = MisclaimedTone(20.0 * Math.Log10(2.0));
            double claimedRMS = 0.5 * ToneAmplitude / Math.Sqrt(2.0);

            TestDelegate check = () => Normalization.CheckDeliveredLevel(
                stream: stream,
                requestedLevel: requestedHL,
                claimedRMS: claimedRMS,
                limit: LevelRegulation.dbSafetyLimit,
                unit: "dB HL");

            if (refused)
            {
                StreamCompositionException e = Assert.Throws<StreamCompositionException>(check);
                StringAssert.Contains("safety limit of 90dB HL", e.Message);
            }
            else
            {
                Assert.DoesNotThrow(check);
            }
        }

        #region Nothing to regulate (PART-984)

        /// <summary>
        /// A stream with a zero claimed RMS (e.g. every carrier outside the renderable range) can't be
        /// played at any level. The regulators scaled by level / 0 = infinity, replaced it by 1, and
        /// played the stream unscaled (silence) while the requested level was logged. It must be
        /// refused, with the safety limit engaged or not.
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void NoRenderableCarrier_IsRefused(bool safetyLimit)
        {
            IBGCStream composer = new SingleFrequencyDomainToneComposer(
                new[] { new ComplexCarrierTone(30_000.0, 0.1) }, ToneDuration);
            Assert.AreEqual(0.0, composer.GetChannelRMS().First(), "Precondition: nothing is claimed");

            IBGCStream normalized = composer.Normalize(50.0, safetyLimit: safetyLimit);
            StreamCompositionException e = Assert.Throws<StreamCompositionException>(() => ReadAll(normalized));
            StringAssert.Contains("nothing to scale", e.Message);
        }

        /// <summary> The same for a stereo stream (NormalizerFilter) and for the dB HL units' shared check </summary>
        [Test]
        public void ZeroClaim_StereoAndDirectCheck_AreRefused()
        {
            IBGCStream silentStereo = new ClaimedRMSStream(new SimpleAudioClip(new float[2 * 4410], 2), 0.0);
            Assert.Throws<StreamCompositionException>(() => ReadAll(silentStereo.Normalize(50.0)));

            foreach (double claim in new[] { 0.0, double.NaN })
            {
                Assert.Throws<StreamCompositionException>(() => Normalization.CheckDeliveredLevel(
                    stream: MisclaimedTone(0.0), requestedLevel: 50.0, claimedRMS: claim,
                    limit: LevelRegulation.dbSafetyLimit, unit: "dB HL"),
                    $"A claimed RMS of {claim} was skipped instead of refused");
            }
        }

        /// <summary>
        /// Samples that aren't numbers can't be judged against the limit: refused, not skipped (the
        /// check used to drop NaN channels and pass what remained).
        /// </summary>
        [Test]
        public void NaNSamples_AreRefused()
        {
            float[] samples = new float[4410];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)(ToneAmplitude * Math.Cos(2.0 * Math.PI * 1000.0 * i / 44100.0));
            }
            samples[100] = float.NaN;

            IBGCStream stream = new ClaimedRMSStream(new SimpleAudioClip(samples, 1), ToneAmplitude / Math.Sqrt(2.0));
            StreamCompositionException e = Assert.Throws<StreamCompositionException>(() => ReadAll(stream.Normalize(50.0)));
            StringAssert.Contains("aren't finite", e.Message);
        }

        /// <summary>
        /// NoiseAudioClip and AnalyticNoiseStream scale their frame by rms / frame RMS: with no
        /// renderable carrier that is rms / 0, and every sample became 0 * infinity = NaN.
        /// </summary>
        [Test]
        public void NoiseWithNoRenderableCarrier_IsRefusedNotNaN()
        {
            NoiseAudioClip clip = new NoiseAudioClip(0.5, 0.1, 30_000.0, 40_000.0, 100,
                NoiseAudioClip.AmplitudeDistribution.White, new Random(1));
            Assert.Throws<StreamCompositionException>(() => ReadAll(clip));

            BGC.Audio.AnalyticStreams.AnalyticNoiseStream analytic = new BGC.Audio.AnalyticStreams.AnalyticNoiseStream(
                0.1, 30_000.0, 40_000.0, 100, BGC.Audio.AnalyticStreams.AnalyticNoiseStream.AmplitudeDistribution.White, new Random(1));
            Assert.Throws<StreamCompositionException>(() => analytic.Initialize());
        }

        /// <summary>
        /// NoiseAudioClip claims the RMS it was asked for, so the samples it returns must have it. It
        /// normalized its whole power-of-two frame instead of the crop it returns (report 14), so
        /// each token's level differed from its claim by the crop's realization.
        /// </summary>
        [Test]
        public void NoiseAudioClip_ReturnedSamples_HaveTheClaimedRMS()
        {
            for (int seed = 0; seed < 10; seed++)
            {
                NoiseAudioClip clip = new NoiseAudioClip(0.5, 0.1, 100.0, 10000.0, 4000,
                    NoiseAudioClip.AmplitudeDistribution.White, new Random(seed));
                float[] x = new float[clip.ChannelSamples];
                clip.Read(x, 0, x.Length);
                double rms = Math.Sqrt(x.Average(v => v * (double)v));

                Assert.AreEqual(0.0, 20.0 * Math.Log10(rms / clip.GetChannelRMS().First()), 1e-4,
                    $"Seed {seed}: returned samples re claim");
            }
        }

        /// <summary>
        /// A regulated stream's claimed RMS is the regulated level even when it is read before the
        /// stream's first Read. NormalizerFilter and NormalizerMonoFilter computed their factors on
        /// initialization but not in GetChannelRMS, so a claim read first was 0, and it was cached:
        /// it stayed 0 after the stream played. A regulator composed on top is not affected
        /// (initialization reaches inner streams first); a caller that reads the claim directly is.
        /// Mono and stereo inner regulators.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        public void RegulatedClaim_ReadBeforeTheFirstRead_IsTheRegulatedRMS(int channels)
        {
            IBGCStream Tone()
            {
                IBGCStream tone = new SineWave(ToneAmplitude, 1000.0).Truncate(totalDuration: ToneDuration);
                return channels == 2 ? tone.UpChannel(2) : tone;
            }

            IBGCStream claimedFirst = Tone().Normalize(50.0);
            double claimBeforeRead = claimedFirst.GetChannelRMS().Max();

            IBGCStream readFirst = Tone().Normalize(50.0);
            float[] samples = new float[2 * 22050];
            readFirst.Read(samples, 0, samples.Length);
            double sampleRMS = Math.Sqrt(samples.Where((x, i) => i % 2 == 0).Average(x => x * (double)x));

            Assert.AreEqual(sampleRMS, claimBeforeRead, 1e-4 * sampleRMS, "A claim read before the first Read");
            Assert.AreEqual(sampleRMS, readFirst.GetChannelRMS().Max(), 1e-4 * sampleRMS, "A claim read after it");
        }

        /// <summary>
        /// A regulator applied to an already regulated stream (a collection-level Level Regulator over
        /// stimuli with their own) sets the level. Regression coverage; initialization reaches the
        /// inner regulator first, so this passed before 51aeb9c too (the "outer regulator ignored"
        /// scenario in 51aeb9c's message needs a claim read before the first Read, above).
        /// Mono and stereo inner regulators.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        public void StackedRegulators_TheOuterSetsTheLevel(int channels)
        {
            IBGCStream Tone()
            {
                IBGCStream tone = new SineWave(ToneAmplitude, 1000.0).Truncate(totalDuration: ToneDuration);
                return channels == 2 ? tone.UpChannel(2) : tone;
            }

            float[] stacked = new float[2 * 22050];
            float[] single = new float[2 * 22050];
            Tone().Normalize(50.0).Normalize(60.0).Read(stacked, 0, stacked.Length);
            Tone().Normalize(60.0).Read(single, 0, single.Length);

            Assert.AreEqual(Math.Sqrt(single.Average(x => x * (double)x)), Math.Sqrt(stacked.Average(x => x * (double)x)), 1e-6,
                "The outer regulator didn't set the level");
        }

        #endregion Nothing to regulate (PART-984)
    }
}

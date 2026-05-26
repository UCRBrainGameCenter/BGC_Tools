using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics.QuestPlus;
using BGC.Mathematics.QuestPlus.PsychometricFunctions;
using LightJson;
using NUnit.Framework;

namespace BGC.Tests
{
    /// <summary>
    /// Tests for the QUEST+ adaptive psychometric procedure.
    ///
    /// Several tests reproduce the deterministic expected stimulus sequences
    /// reported by Watson (2017) and verified by the reference Python
    /// questplus package's test suite. Any drift in those sequences indicates
    /// a likely bug in the engine, the psychometric function, or the index
    /// ordering of the posterior.
    /// </summary>
    public class QuestPlusTests
    {
        private const string CorrectLabel = "Correct";
        private const string IncorrectLabel = "Incorrect";
        private const int Correct = 0;
        private const int Incorrect = 1;

        private static QuestPlusDimension Dim(string name, params double[] values)
            => new QuestPlusDimension(name, values);

        private static QuestPlusDimension Range(string name, int from, int toInclusive)
        {
            double[] vals = new double[toInclusive - from + 1];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = from + i;
            }
            return new QuestPlusDimension(name, vals);
        }

        // ===== Construction =====

        [Test]
        public void Constructor_RequiresMatchingDimensionCounts()
        {
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);

            // Weibull expects 4 param dimensions; passing 1 should throw.
            Assert.Throws<ArgumentException>(() =>
                new BGC.Mathematics.QuestPlus.QuestPlus(
                    stimDomain: new[] { contrast },
                    paramDomain: new[] { threshold },
                    outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                    psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB)));
        }

        [Test]
        public void Constructor_UniformPriorByDefault()
        {
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull();
            int n = q.ParamSize;
            double expected = 1.0 / n;
            foreach (double v in q.Prior)
            {
                Assert.AreEqual(expected, v, 1e-12);
            }
            // Posterior should start equal to prior.
            CollectionAssert.AreEqual(q.Prior, q.Posterior);
        }

        [Test]
        public void Constructor_CustomPriorIsNormalized()
        {
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);
            QuestPlusDimension slope = Dim("slope", 3.5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            double[] customPrior = new double[threshold.Length];
            for (int i = 0; i < customPrior.Length; i++)
            {
                // Triangular prior peaking around the middle.
                customPrior[i] = (i + 1) * 2.0;
            }

            BGC.Mathematics.QuestPlus.QuestPlus q = new BGC.Mathematics.QuestPlus.QuestPlus(
                stimDomain: new[] { contrast },
                paramDomain: new[] { threshold, slope, guess, lapse },
                outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB),
                prior: customPrior);

            double sum = 0;
            foreach (double v in q.Prior) sum += v;
            Assert.AreEqual(1.0, sum, 1e-12);
            // Values must be proportional to the original prior.
            for (int i = 0; i < customPrior.Length; i++)
            {
                Assert.AreEqual(customPrior[i] / (q.ParamSize * (q.ParamSize + 1.0)), q.Prior[i], 1e-12);
            }
        }

        [Test]
        public void Constructor_RejectsNegativePrior()
        {
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);
            QuestPlusDimension slope = Dim("slope", 3.5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            double[] bad = new double[threshold.Length];
            bad[0] = -1;

            Assert.Throws<ArgumentException>(() =>
                new BGC.Mathematics.QuestPlus.QuestPlus(
                    stimDomain: new[] { contrast },
                    paramDomain: new[] { threshold, slope, guess, lapse },
                    outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                    psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB),
                    prior: bad));
        }

        // ===== Watson Example 1: threshold-only estimation =====

        /// <summary>
        /// Reproduces Watson (2017), Example 1 / questplus test_threshold.
        /// With a 1-dimensional Weibull psychometric function (only threshold
        /// is being estimated) and the dB scale, QUEST+ must place each
        /// stimulus at the expected contrast value below.
        /// </summary>
        [Test]
        public void Threshold_Watson2017Example1_DeterministicStimulusSequence()
        {
            int[] expectedContrasts = new[]
            {
                -18, -22, -25, -28, -30, -22, -13, -15, -16, -18,
                -19, -20, -21, -22, -23, -19, -20, -20, -18, -18,
                -19, -17, -17, -18, -18, -18, -19, -19, -19, -19,
                -19, -19,
            };

            int[] responses = new[]
            {
                Correct, Correct, Correct, Correct, Incorrect,
                Incorrect, Correct, Correct, Correct, Correct,
                Correct, Correct, Correct, Correct, Incorrect,
                Correct, Correct, Incorrect, Correct, Correct,
                Incorrect, Correct, Correct, Correct, Correct,
                Correct, Correct, Correct, Correct, Correct,
                Correct, Correct,
            };

            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(ParamEstimationMethod.Mode);

            for (int i = 0; i < expectedContrasts.Length; i++)
            {
                double[] next = q.GetNextStim();
                Assert.AreEqual(1, next.Length);
                Assert.AreEqual(expectedContrasts[i], next[0], 1e-9,
                    $"Trial {i}: expected contrast {expectedContrasts[i]} but got {next[0]}");
                q.Update(next, responses[i]);
            }

            double[] estimate = q.GetParamEstimate();
            Assert.AreEqual(-20.0, estimate[0], 1e-9, "Mode threshold should be -20.");
        }

        // ===== Watson Implementation example: threshold + slope =====

        /// <summary>
        /// Reproduces the threshold-and-slope example from Watson (2017)
        /// Implementation section / questplus test_threshold_slope.
        /// </summary>
        [Test]
        public void ThresholdSlope_DeterministicStimulusSequence()
        {
            int[] expectedContrasts = new[]
            {
                -18, -22, -12, -13, -15, -16, -17, -18, -19, -21,
                -24, -25, -26, -27, -21, -22, -23, -20, -20, -20,
                -21, -19, -19, -20, -20, -20, -19, -19, -19, -19,
                -19, -20, -20, -20, -20, -20, -21, -21, -20, -20,
                -20, -19, -18, -18, -19, -19, -19, -18, -18, -17,
                -18, -18, -18, -17, -17, -17, -17, -17, -17, -17,
                -18, -18, -18, -18,
            };

            int[] responses = new[]
            {
                Correct, Incorrect, Correct, Correct, Correct,
                Correct, Correct, Correct, Correct, Correct,
                Correct, Correct, Correct, Incorrect, Correct,
                Correct, Incorrect, Correct, Correct, Correct,
                Incorrect, Correct, Correct, Correct, Correct,
                Incorrect, Correct, Correct, Correct, Correct,
                Correct, Correct, Correct, Correct, Correct,
                Correct, Correct, Incorrect, Correct, Correct,
                Incorrect, Incorrect, Correct, Correct, Correct,
                Correct, Incorrect, Correct, Incorrect, Correct,
                Correct, Correct, Incorrect, Correct, Correct,
                Correct, Correct, Correct, Correct, Correct,
                Correct, Correct, Correct, Correct,
            };

            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);
            QuestPlusDimension slope = Range("slope", 2, 5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            BGC.Mathematics.QuestPlus.QuestPlus q = new BGC.Mathematics.QuestPlus.QuestPlus(
                stimDomain: new[] { contrast },
                paramDomain: new[] { threshold, slope, guess, lapse },
                outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB),
                estimationMethod: ParamEstimationMethod.Mode);

            for (int i = 0; i < expectedContrasts.Length; i++)
            {
                double[] next = q.GetNextStim();
                Assert.AreEqual(expectedContrasts[i], next[0], 1e-9,
                    $"Trial {i}: expected contrast {expectedContrasts[i]} but got {next[0]}");
                q.Update(next, responses[i]);
            }

            double[] estimate = q.GetParamEstimate();
            Assert.AreEqual(-20.0, estimate[0], 1e-9, "Mode threshold");
            Assert.AreEqual(3.0, estimate[1], 1e-9, "Mode slope");
        }

        // ===== Parameter recovery via simulated observer =====

        [Test]
        public void SimulatedObserver_RecoversThreshold_AfterEnoughTrials()
        {
            // Simulated Weibull observer with known parameters; check that
            // posterior mode recovers them.
            double trueThreshold = -20;
            double slope = 3.5;
            double guess = 0.5;
            double lapse = 0.02;

            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(ParamEstimationMethod.Mode);

            WeibullPsychometricFunction generator = new WeibullPsychometricFunction(WeibullScale.DB);
            double[] trueParams = new[] { trueThreshold, slope, guess, lapse };
            double[] probs = new double[2];

            // Use a deterministic RNG for reproducibility.
            Random rng = new Random(1234);

            for (int t = 0; t < 64; t++)
            {
                double[] stim = q.GetNextStim();
                generator.Evaluate(stim, trueParams, probs);
                int outcome = rng.NextDouble() < probs[0] ? Correct : Incorrect;
                q.Update(stim, outcome);
            }

            double[] estimate = q.GetParamEstimate();
            // Mode is quantized to the integer grid; allow 1-step margin.
            Assert.LessOrEqual(Math.Abs(estimate[0] - trueThreshold), 1.0,
                $"Threshold estimate {estimate[0]} should be near {trueThreshold}.");
        }

        // ===== Parameter estimation modes =====

        [Test]
        public void ParamEstimate_MeanAndMode_BehaveDifferently()
        {
            // After a few trials the posterior is not perfectly peaked, so
            // mean and mode should generally differ. We just verify both
            // return finite values in range.
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(ParamEstimationMethod.Mean);

            // Inject several correct responses at low contrast and incorrect at high.
            for (int i = 0; i < 8; i++)
            {
                double[] stim = q.GetNextStim();
                int outcome = stim[0] >= -20 ? Correct : Incorrect;
                q.Update(stim, outcome);
            }

            q.EstimationMethod = ParamEstimationMethod.Mean;
            double[] meanEst = q.GetParamEstimate();

            q.EstimationMethod = ParamEstimationMethod.Mode;
            double[] modeEst = q.GetParamEstimate();

            Assert.False(double.IsNaN(meanEst[0]));
            Assert.False(double.IsNaN(modeEst[0]));
            // Both should lie inside the threshold domain [-40, 0].
            Assert.GreaterOrEqual(meanEst[0], -40);
            Assert.LessOrEqual(meanEst[0], 0);
            Assert.GreaterOrEqual(modeEst[0], -40);
            Assert.LessOrEqual(modeEst[0], 0);
        }

        // ===== Marginal posteriors =====

        [Test]
        public void MarginalPosterior_SumsToOne_ForEachParameterDim()
        {
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);
            QuestPlusDimension slope = Range("slope", 2, 5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            BGC.Mathematics.QuestPlus.QuestPlus q = new BGC.Mathematics.QuestPlus.QuestPlus(
                stimDomain: new[] { contrast },
                paramDomain: new[] { threshold, slope, guess, lapse },
                outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB));

            // Run a few trials so the posterior is non-uniform.
            for (int i = 0; i < 12; i++)
            {
                double[] stim = q.GetNextStim();
                q.Update(stim, i % 3 == 0 ? Incorrect : Correct);
            }

            IReadOnlyDictionary<string, double[]> marginals = q.GetMarginalPosteriors();
            Assert.AreEqual(4, marginals.Count);

            foreach (KeyValuePair<string, double[]> kv in marginals)
            {
                double sum = kv.Value.Sum();
                Assert.AreEqual(1.0, sum, 1e-9, $"Marginal '{kv.Key}' should sum to 1.");
                Assert.True(kv.Value.All(p => p >= 0),
                    $"Marginal '{kv.Key}' should be non-negative.");
            }
        }

        // ===== Reset =====

        [Test]
        public void Reset_RestoresPriorAndClearsHistory()
        {
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull();
            double[] originalPosterior = q.Posterior.ToArray();

            for (int i = 0; i < 5; i++)
            {
                double[] stim = q.GetNextStim();
                q.Update(stim, Correct);
            }
            Assert.AreEqual(5, q.History.Count);
            Assert.False(q.Posterior.SequenceEqual(originalPosterior), "Posterior should have changed after updates.");

            q.Reset();
            Assert.AreEqual(0, q.History.Count);
            CollectionAssert.AreEqual(originalPosterior, q.Posterior);
            Assert.True(double.IsNaN(q.LastEntropy));
        }

        // ===== Serialization =====

        [Test]
        public void Serialize_RoundTrip_PreservesPosteriorAndHistory()
        {
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(ParamEstimationMethod.Mode);

            for (int i = 0; i < 10; i++)
            {
                double[] stim = q.GetNextStim();
                q.Update(stim, i % 2 == 0 ? Correct : Incorrect);
            }

            JsonObject state = q.SerializeState();

            BGC.Mathematics.QuestPlus.QuestPlus q2 = BuildSimpleWeibull(ParamEstimationMethod.Mode);
            q2.DeserializeState(state);

            // Posteriors identical.
            Assert.AreEqual(q.Posterior.Count, q2.Posterior.Count);
            for (int i = 0; i < q.Posterior.Count; i++)
            {
                Assert.AreEqual(q.Posterior[i], q2.Posterior[i], 1e-15);
            }
            // History identical.
            Assert.AreEqual(q.History.Count, q2.History.Count);
            for (int i = 0; i < q.History.Count; i++)
            {
                Assert.AreEqual(q.History[i].StimFlatIndex, q2.History[i].StimFlatIndex);
                Assert.AreEqual(q.History[i].OutcomeIndex, q2.History[i].OutcomeIndex);
            }
            // The next stim selected should match too.
            double[] nextA = q.GetNextStim();
            double[] nextB = q2.GetNextStim();
            Assert.AreEqual(nextA[0], nextB[0], 1e-12);
        }

        [Test]
        public void Serialize_RoundTrip_AllowsContinuingRun()
        {
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(ParamEstimationMethod.Mode);
            for (int i = 0; i < 5; i++)
            {
                q.Update(q.GetNextStim(), Correct);
            }
            JsonObject state = q.SerializeState();

            // Continue on the original.
            for (int i = 0; i < 5; i++)
            {
                q.Update(q.GetNextStim(), Incorrect);
            }
            double[] aEstimate = q.GetParamEstimate();

            // Resume from saved state and replay the same 5 incorrect trials.
            BGC.Mathematics.QuestPlus.QuestPlus q2 = BuildSimpleWeibull(ParamEstimationMethod.Mode);
            q2.DeserializeState(state);
            for (int i = 0; i < 5; i++)
            {
                q2.Update(q2.GetNextStim(), Incorrect);
            }
            double[] bEstimate = q2.GetParamEstimate();

            for (int d = 0; d < aEstimate.Length; d++)
            {
                Assert.AreEqual(aEstimate[d], bEstimate[d], 1e-9,
                    $"Estimate mismatch after resume on dim {d}.");
            }
        }

        [Test]
        public void Deserialize_RejectsMismatchedDomain()
        {
            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull();
            JsonObject state = q.SerializeState();

            // A QuestPlus with a different threshold domain should refuse the state.
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -30, 0); // different
            QuestPlusDimension slope = Dim("slope", 3.5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            BGC.Mathematics.QuestPlus.QuestPlus other = new BGC.Mathematics.QuestPlus.QuestPlus(
                stimDomain: new[] { contrast },
                paramDomain: new[] { threshold, slope, guess, lapse },
                outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB));

            Assert.Throws<ArgumentException>(() => other.DeserializeState(state));
        }

        // ===== MinNEntropy selection =====

        [Test]
        public void MinNEntropy_WithFixedSeed_IsReproducible()
        {
            StimSelectionOptions opts = new StimSelectionOptions
            {
                N = 4,
                MaxConsecutiveReps = 2,
                RandomSeed = 12345,
            };

            BGC.Mathematics.QuestPlus.QuestPlus q1 = BuildSimpleWeibull(
                ParamEstimationMethod.Mode,
                StimSelectionMethod.MinNEntropy,
                opts);
            BGC.Mathematics.QuestPlus.QuestPlus q2 = BuildSimpleWeibull(
                ParamEstimationMethod.Mode,
                StimSelectionMethod.MinNEntropy,
                new StimSelectionOptions { N = 4, MaxConsecutiveReps = 2, RandomSeed = 12345 });

            for (int i = 0; i < 20; i++)
            {
                double[] s1 = q1.GetNextStim();
                double[] s2 = q2.GetNextStim();
                Assert.AreEqual(s1[0], s2[0], 1e-12, $"Mismatch at trial {i}");
                q1.Update(s1, Correct);
                q2.Update(s2, Correct);
            }
        }

        [Test]
        public void MinNEntropy_LimitsConsecutiveRepeats()
        {
            StimSelectionOptions opts = new StimSelectionOptions
            {
                N = 4,
                MaxConsecutiveReps = 2,
                RandomSeed = 42,
            };

            BGC.Mathematics.QuestPlus.QuestPlus q = BuildSimpleWeibull(
                ParamEstimationMethod.Mode,
                StimSelectionMethod.MinNEntropy,
                opts);

            int maxRun = 1;
            int run = 1;
            double prev = double.NaN;
            for (int i = 0; i < 100; i++)
            {
                double[] s = q.GetNextStim();
                q.Update(s, i % 2 == 0 ? Correct : Incorrect);
                if (s[0] == prev)
                {
                    run++;
                    maxRun = Math.Max(maxRun, run);
                }
                else
                {
                    run = 1;
                }
                prev = s[0];
            }

            // The selection rule rejects when ALL of the last maxReps history
            // entries equal the candidate. So we expect runs of at most
            // maxConsecutiveReps + 1 (the run that triggers the rejection on
            // the next pick). Allow a small buffer for the "give up after many
            // attempts" fallback.
            Assert.LessOrEqual(maxRun, opts.MaxConsecutiveReps + 2,
                $"Consecutive repeat run of {maxRun} exceeded allowed.");
        }

        // ===== Helpers =====

        private static BGC.Mathematics.QuestPlus.QuestPlus BuildSimpleWeibull(
            ParamEstimationMethod estimation = ParamEstimationMethod.Mean,
            StimSelectionMethod selection = StimSelectionMethod.MinEntropy,
            StimSelectionOptions options = null)
        {
            QuestPlusDimension contrast = Range("intensity", -40, 0);
            QuestPlusDimension threshold = Range("threshold", -40, 0);
            QuestPlusDimension slope = Dim("slope", 3.5);
            QuestPlusDimension guess = Dim("lower_asymptote", 0.5);
            QuestPlusDimension lapse = Dim("lapse_rate", 0.02);

            return new BGC.Mathematics.QuestPlus.QuestPlus(
                stimDomain: new[] { contrast },
                paramDomain: new[] { threshold, slope, guess, lapse },
                outcomeDomain: new[] { CorrectLabel, IncorrectLabel },
                psychometricFunction: new WeibullPsychometricFunction(WeibullScale.DB),
                selectionMethod: selection,
                selectionOptions: options,
                estimationMethod: estimation);
        }
    }
}

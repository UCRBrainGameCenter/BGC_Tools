using System;
using System.Linq;
using BGC.AdaptiveDifficultyAlgorithm;
using BGC.Parameters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BGC.Tests
{
    public class AdaptiveDifficultyAlgorithmTests
    {
        [IntFieldDisplay("StartingDifficulty", "Starting Level", initial: 2, minimum: 2, maximum: 10)]
        [IntFieldDisplay("MaximumDifficulty", "Maximum Level", initial: 10, minimum: 2, maximum: 10)]
        [IntFieldDisplay("TrialsPerLevel", "Trials Per Level", initial: 2, minimum: 1)]
        [IntFieldDisplay("MinimumCorrectToAdvance", "Minimum Correct to Advance", initial: 1, minimum: 1)]
        [IntFieldDisplay("AlwaysAdvancedToDifficulty", "Always Advance To Level", initial: 5, minimum: 2)]
        [BoolDisplay("ShortCircuiting", "Short Circuiting", initial: false)]
        public class TestStandardAlgorithmClass : StandardDifficultyProgressionAlgorithm
        {
        }


        [Test]
        public void TestStandardAlgorithmNoShortCircuiting()
        {
            TestStandardAlgorithmClass algo = new TestStandardAlgorithmClass()
            {
                StartingDifficulty = 2,
                MaximumDifficulty = 10,
                TrialsPerLevel = 2,
                MinimumCorrectToAdvance = 1,
                AlwaysAdvancedToDifficulty = 5,
                ShortCircuiting = false,
            };
            algo.Initialize();

            int maximumLevels = algo.MaximumDifficulty - algo.StartingDifficulty + 1;
            int minimumLevels = algo.AlwaysAdvancedToDifficulty - algo.StartingDifficulty + 1;

            // 100% success rate should do exactly 18 trials with a threshold of 10
            Debug.Log($"[TestStandardAlgorithmNoShortCircuiting] Running 100% success");
            int numTrials = 1;
            while (algo.SubmitTrialResult(true))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == algo.TrialsPerLevel * maximumLevels);
            Debug.Assert(algo.Threshold == algo.MaximumDifficulty);

            // 100% failure rate should do exactly 8 trials with a threshold of 1
            Debug.Log($"[TestStandardAlgorithmNoShortCircuiting] Running 100% failure");
            algo.Initialize();
            numTrials = 1;
            while (algo.SubmitTrialResult(false))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == algo.TrialsPerLevel * minimumLevels);
            Debug.Assert(algo.Threshold == algo.StartingDifficulty - 1);

            // Fixed scenarios
            (bool[], int)[] trialResults = new (bool[], int)[]
            {
                (new bool[] { true, true, true, true, true, true, true, true, true, true, false, false }, 6),
                (new bool[] { true, false, true, false, true, false, true, false, true, false, false, false }, 6),
                (new bool[] { false, true, false, true, false, true, false, true, false, true, false, false }, 6),
                (new bool[] { false, false, false, false, false, false, false, true, false, true, false, true, false, false }, 7),
                (new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, false }, 9),
                (new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false }, 10),
            };
            for (int i = 0; i < trialResults.Length; i++)
            {
                Debug.Log($"[TestStandardAlgorithmNoShortCircuiting] Running fixed trial {i}");
                algo.Initialize();
                (bool[] trials, int expectedThreshold) = trialResults[i];
                for (int trialIndex = 0; trialIndex < trials.Length; trialIndex++)
                {
                    bool shouldContinue = algo.SubmitTrialResult(trials[trialIndex]);
                    bool isFinalTrial = (trialIndex == trials.Length - 1);
                    Debug.Assert(!shouldContinue == isFinalTrial);
                }
                Debug.Assert(algo.IsDone);
                Debug.Assert(algo.Threshold == expectedThreshold);
            }
        }

        [Test]
        public void TestStandardAlgorithmWithShortCircuiting()
        {
            TestStandardAlgorithmClass algo = new TestStandardAlgorithmClass()
            {
                StartingDifficulty = 2,
                MaximumDifficulty = 10,
                TrialsPerLevel = 2,
                MinimumCorrectToAdvance = 1,
                AlwaysAdvancedToDifficulty = 5,
                ShortCircuiting = true,
            };
            algo.Initialize();

            int maximumLevels = algo.MaximumDifficulty - algo.StartingDifficulty + 1;
            int minimumLevels = algo.AlwaysAdvancedToDifficulty - algo.StartingDifficulty + 1;

            // 100% success rate should do exactly 18 trials with a threshold of 10
            Debug.Log($"[TestStandardAlgorithmWithShortCircuiting] Running 100% success");
            int numTrials = 1;
            while (algo.SubmitTrialResult(true))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == maximumLevels);
            Debug.Assert(algo.Threshold == algo.MaximumDifficulty);

            // 100% failure rate should do exactly 8 trials with a threshold of 1
            Debug.Log($"[TestStandardAlgorithmWithShortCircuiting] Running 100% failure");
            algo.Initialize();
            numTrials = 1;
            while (algo.SubmitTrialResult(false))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == algo.TrialsPerLevel * minimumLevels);
            Debug.Assert(algo.Threshold == algo.StartingDifficulty - 1);

            // Fixed scenarios
            (bool[], int)[] trialResults = new (bool[], int)[]
            {
                (new bool[] { true, true, true, true, true, false, false }, 6),
                (new bool[] { false, true, false, true, false, true, false, true, false, true, false, false }, 6),
                (new bool[] { false, false, false, false, false, false, false, true, true, true, false, false }, 7),
                (new bool[] { true, true, true, true, true, true, true, true, false, false }, 9),
                (new bool[] { true, true, true, true, true, true, true, true, false, true }, 10),
            };
            for (int i = 0; i < trialResults.Length; i++)
            {
                Debug.Log($"[TestStandardAlgorithmWithShortCircuiting] Running fixed trial {i}");
                algo.Initialize();
                (bool[] trials, int expectedThreshold) = trialResults[i];
                for (int trialIndex = 0; trialIndex < trials.Length; trialIndex++)
                {
                    bool shouldContinue = algo.SubmitTrialResult(trials[trialIndex]);
                    bool isFinalTrial = (trialIndex == trials.Length - 1);
                    Debug.Assert(!shouldContinue == isFinalTrial);
                }
                Debug.Assert(algo.IsDone);
                Debug.Assert(algo.Threshold == expectedThreshold);
            }
        }

        [IntFieldDisplay("NumLives", "NumLives", initial: 2, minimum: 1)]
        [IntFieldDisplay("MinimumDifficulty", "MinimumDifficulty", initial: 2, minimum: 2, maximum: 10)]
        [IntFieldDisplay("MaximumDifficulty", "MaximumDifficulty", initial: 10, minimum: 2, maximum: 10)]
        [IntFieldDisplay("MinimumCorrectToAdvance", "MinimumCorrectToAdvance", initial: 1, minimum: 1)]
        [IntFieldDisplay("NumConsecutiveIncorrectToLoseLife", "NumConsecutiveIncorrectToLoseLife", initial: 2, minimum: 1)]
        [IntFieldDisplay("MinCorrectAtMaxDifficultyToStop", "MinCorrectAtMaxDifficultyToStop", initial: 2, minimum: 1)]
        class TestLivesAlgorithmTestClass : LivesDifficultyAlgorithm { }

        [Test]
        public void TestLivesAlgorithmTest()
        {
            TestLivesAlgorithmTestClass algo = new TestLivesAlgorithmTestClass()
            {
                NumLives = 2,
                MinimumDifficulty = 2,
                MaximumDifficulty = 10,
                MinimumCorrectToAdvance = 1,
                NumConsecutiveIncorrectToLoseLife = 2,
                MinCorrectAtMaxDifficultyToStop = 2,
            };
            algo.Initialize();

            // 100% success rate should do one of each trial up to difficulty 10, then one more to confirm 10.
            Debug.Log($"[TestLivesAlgorithmTest] Running 100% success");
            int numTrials = 1;
            while (algo.SubmitTrialResult(true))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == algo.MaximumDifficulty - algo.MinimumDifficulty + 2);
            Debug.Assert(algo.Threshold == algo.MaximumDifficulty);

            // 100% failure rate should lose a life after 2 trials, then lose the second after 4 trials and be done.
            Debug.Log($"[TestLivesAlgorithmTest] Running 100% failure");
            algo.Initialize();
            numTrials = 1;
            while (algo.SubmitTrialResult(false))
            {
                Debug.Assert(!algo.IsDone);
                numTrials++;

                // Don't let it infinite loop
                Debug.Assert(numTrials < 1000);
                if (numTrials >= 1000)
                {
                    break;
                }
            }
            Debug.Assert(algo.IsDone);
            Debug.Assert(numTrials == algo.NumLives * algo.NumConsecutiveIncorrectToLoseLife);
            Debug.Assert(algo.Threshold == algo.MinimumDifficulty - 1);

            // Fixed scenarios
            (bool[], int)[] trialResults = new (bool[], int)[]
            {
                (new bool[] { true, false, true, false, false, true, true, false, false }, 3),
                (new bool[] { true, true, true, true, false, false, false, false }, 5),
                (new bool[] { true, true, true, true, false, true, false, false, true, true, false, false }, 6),
                (new bool[] { true, true, true, true, false, true, true, false, false, true, true, false, false }, 7),
                (new bool[] { true, true, true, true, false, true, false, false, true, true, true, false, false }, 7),
            };
            for (int i = 0; i < trialResults.Length; i++)
            {
                Debug.Log($"[TestLivesAlgorithmTest] Running fixed trial {i}");
                algo.Initialize();
                (bool[] trials, int expectedThreshold) = trialResults[i];
                for (int trialIndex = 0; trialIndex < trials.Length; trialIndex++)
                {
                    bool shouldContinue = algo.SubmitTrialResult(trials[trialIndex]);
                    bool isFinalTrial = (trialIndex == trials.Length - 1);
                    Debug.Assert(!shouldContinue == isFinalTrial);
                }
                Debug.Assert(algo.IsDone);
                Debug.Log($"Threshold: {algo.Threshold} expected: {expectedThreshold}");
                Debug.Assert(algo.Threshold == expectedThreshold);
            }
        }

        [StringFieldDisplay("Trials", "Trials", "2, 3, 4, 5, 6, 7, 8, 9, 10")]
        class FixedTrialsAlgorithmTestClass : FixedTrialsDifficultyAlgorithm { }

        [Test]
        public void FixedTrialsAlgorithmTest()
        {
            FixedTrialsAlgorithmTestClass algo = new FixedTrialsAlgorithmTestClass()
            {
                Trials = "2, 3, 4, 5"
            };
            algo.Initialize();

            Debug.Assert(algo.Difficulty == 2);
            bool submitResult = algo.SubmitTrialResult(true);
            Debug.Assert(submitResult);
            Debug.Assert(!algo.IsDone);
            Debug.Assert(algo.Threshold == 2);

            Debug.Assert(algo.Difficulty == 3);
            submitResult = algo.SubmitTrialResult(true);
            Debug.Assert(submitResult);
            Debug.Assert(!algo.IsDone);
            Debug.Assert(algo.Threshold == 3);

            Debug.Assert(algo.Difficulty == 4);
            submitResult = algo.SubmitTrialResult(true);
            Debug.Assert(submitResult);
            Debug.Assert(!algo.IsDone);
            Debug.Assert(algo.Threshold == 4);

            Debug.Assert(algo.Difficulty == 5);
            submitResult = algo.SubmitTrialResult(false);
            Debug.Assert(!submitResult);
            Debug.Assert(algo.IsDone);
            Debug.Assert(algo.Threshold == 4);
        }
    }
}

using System;
using BGC.Parameters;

namespace BGC.AdaptiveDifficultyAlgorithm
{
    [PropertyChoiceTitle("Lives-based Progression")]
    public class LivesDifficultyAlgorithm : AdaptiveDifficultyAlgorithmBase
    {
        // Parameters
        [DisplayInputField("NumLives")]
        public int NumLives { get; set; }

        [DisplayInputField("MinimumDifficulty")]
        public int MinimumDifficulty { get; set; }

        [DisplayInputField("MaximumDifficulty")]
        public int MaximumDifficulty { get; set; }

        [DisplayInputField("MinimumCorrectToAdvance")]
        public int MinimumCorrectToAdvance { get; set; }

        [DisplayInputField("NumConsecutiveIncorrectToLoseLife")]
        public int NumConsecutiveIncorrectToLoseLife { get; set; }

        [DisplayInputField("MinCorrectAtMaxDifficultyToStop")]
        public int MinCorrectAtMaxDifficultyToStop { get; set; }

        // Internals
        private int difficulty;
        private int livesRemaining;
        private int numCorrectThisLevel;
        private int numConsecutiveIncorrectThisLevel;
        private int numCorrectAtMaxLevel;
        private int threshold;
        private bool isDone;

        // IAdaptiveDifficultyAlgorithm implementation
        public override int Difficulty => difficulty;

        public override int Threshold => threshold;

        public override bool IsDone => isDone;

        public override void Initialize()
        {
            difficulty = MinimumDifficulty;
            livesRemaining = NumLives;
            numCorrectThisLevel = 0;
            numConsecutiveIncorrectThisLevel = 0;
            numCorrectAtMaxLevel = 0;
            threshold = MinimumDifficulty - 1;
            isDone = false;
        }

        public override bool SubmitTrialResult(bool correct)
        {
            if (isDone)
            {
                return false;
            }

            if (correct)
            {
                numCorrectThisLevel++;
                numConsecutiveIncorrectThisLevel = 0;
            }
            else
            {
                numConsecutiveIncorrectThisLevel++;
            }

            if (correct && difficulty == MaximumDifficulty)
            {
                numCorrectAtMaxLevel++;
            }

            if (numConsecutiveIncorrectThisLevel >= NumConsecutiveIncorrectToLoseLife)
            {
                // Lose a life, and if any lives remain reduce the difficulty.
                // Otherwise, a threshold has been reached.
                livesRemaining--;
                if (livesRemaining > 0)
                {
                    difficulty = Math.Max(difficulty - 2, MinimumDifficulty);
                    numCorrectThisLevel = 0;
                    numConsecutiveIncorrectThisLevel = 0;
                    return true;
                }

                isDone = true;
                return false;
            }

            if (numCorrectThisLevel >= MinimumCorrectToAdvance)
            {
                // Special case for maximum level: only complete if enough have been correct.
                if (difficulty >= MaximumDifficulty)
                {
                    if (numCorrectAtMaxLevel < MinCorrectAtMaxDifficultyToStop)
                    {
                        // Do more trials at max level.
                        return true;
                    }

                    threshold = difficulty;
                    isDone = true;
                    return false;
                }

                // Update the threshold
                threshold = difficulty;

                // Progress to the next difficulty
                difficulty++;
                numCorrectThisLevel = 0;
                numConsecutiveIncorrectThisLevel = 0;
                return true;
            }

            // Do more trials at this level
            return true;
        }
    }
}

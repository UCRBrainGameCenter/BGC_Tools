using BGC.Parameters;

namespace BGC.AdaptiveDifficultyAlgorithm
{
    [PropertyChoiceTitle("Standard Progression")]
    public class StandardDifficultyProgressionAlgorithm : AdaptiveDifficultyAlgorithmBase
    {
        // Parameters
        [DisplayInputField("StartingDifficulty")]
        public int StartingDifficulty { get; set; }

        [DisplayInputField("MaximumDifficulty")]
        public int MaximumDifficulty { get; set; }

        [DisplayInputField("TrialsPerLevel")]
        public int TrialsPerLevel { get; set; }

        [DisplayInputField("MinimumCorrectToAdvance")]
        public int MinimumCorrectToAdvance { get; set; }

        [DisplayInputField("AlwaysAdvancedToDifficulty")]
        public int AlwaysAdvancedToDifficulty { get; set; }

        [DisplayInputField("ShortCircuiting")]
        public bool ShortCircuiting { get; set; }

        // Internals
        private int difficulty;
        private int numCorrectThisLevel;
        private int numTrialsThisLevel;
        private int threshold;
        private bool isDone;

        // IAdaptiveDifficultyAlgorithm implementation
        public override int Difficulty => difficulty;

        public override int Threshold => threshold;

        public override bool IsDone => isDone;

        public override void Initialize()
        {
            difficulty = StartingDifficulty;
            numCorrectThisLevel = 0;
            numTrialsThisLevel = 0;
            threshold = StartingDifficulty - 1;
            isDone = false;
        }

        public override bool SubmitTrialResult(bool correct)
        {
            if (isDone)
            {
                return false;
            }

            numTrialsThisLevel++;
            numCorrectThisLevel += correct ? 1 : 0;

            if (!ShortCircuiting && numTrialsThisLevel < TrialsPerLevel)
            {
                // Always do a set number of trials per level
                return true;
            }

            bool advancementEarned = numCorrectThisLevel >= MinimumCorrectToAdvance;
            if (advancementEarned)
            {
                // Only update the threshold when advancement was earned
                threshold = difficulty;
            }

            bool forcedAdvance = difficulty < AlwaysAdvancedToDifficulty && numTrialsThisLevel >= TrialsPerLevel;
            if (advancementEarned || forcedAdvance)
            {
                // Advance to the next difficulty.
                difficulty++;
                numCorrectThisLevel = 0;
                numTrialsThisLevel = 0;
                if (difficulty <= MaximumDifficulty)
                {
                    return true;
                }

                // Cannot increase difficulty more, so threshold has been reached
                isDone = true;
                return false;
            }

            if (ShortCircuiting && numTrialsThisLevel < TrialsPerLevel)
            {
                // Continue with more trials at this level
                return true;
            }

            // Threshold has been reached
            isDone = true;
            return false;
        }
    }
}

namespace BGC.Procedures
{
    /// <summary>
    /// Output from adaptive difficulty procedures.
    /// </summary>
    public record DifficultyOutput
    {
        /// <summary>
        /// The difficulty level for the next trial.
        /// Interpretation depends on the procedure and stimulus translator.
        /// </summary>
        public int Difficulty { get; init; }

        /// <summary>
        /// Estimated threshold (e.g., 70.7% correct point).
        /// Null if not yet calculable (insufficient data).
        /// </summary>
        public double? Threshold { get; init; }

        /// <summary>
        /// True if the procedure has completed (e.g., reached required reversals).
        /// </summary>
        public bool IsComplete { get; init; }

        public DifficultyOutput(int difficulty, double? threshold = null, bool isComplete = false)
        {
            Difficulty = difficulty;
            Threshold = threshold;
            IsComplete = isComplete;
        }
    }
}
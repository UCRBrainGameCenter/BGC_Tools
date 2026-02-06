namespace BGC.Procedures
{
    /// <summary>
    /// Outcome of a single trial.
    /// </summary>
    public enum TrialOutcome
    {
        /// <summary>Participant responded correctly.</summary>
        Correct,

        /// <summary>Participant responded incorrectly.</summary>
        Incorrect,

        /// <summary>No response or invalid trial (may be ignored by some procedures).</summary>
        NoResponse
    }

    /// <summary>
    /// Input to adaptive difficulty procedures after a trial completes.
    /// </summary>
    public record TrialResult
    {
        public TrialOutcome Outcome { get; init; }

        public TrialResult(TrialOutcome outcome)
        {
            Outcome = outcome;
        }

        // Optional: response time, confidence, or other metadata
        // public TimeSpan? ResponseTime { get; init; }
    }
}
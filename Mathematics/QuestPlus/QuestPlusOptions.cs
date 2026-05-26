namespace BGC.Mathematics.QuestPlus
{
    /// <summary>
    /// How QUEST+ should pick the next stimulus.
    /// </summary>
    public enum StimSelectionMethod
    {
        /// <summary>Pick the stimulus that minimizes expected entropy.</summary>
        MinEntropy,

        /// <summary>
        /// Randomly pick from the n stimuli with the smallest expected entropies.
        /// </summary>
        MinNEntropy,
    }

    /// <summary>
    /// How QUEST+ should compute the current parameter estimate.
    /// </summary>
    public enum ParamEstimationMethod
    {
        /// <summary>Posterior mean of each parameter (weighted by marginal).</summary>
        Mean,

        /// <summary>Parameter values at the peak of the full posterior.</summary>
        Mode,
    }

    /// <summary>
    /// Optional settings for the MinNEntropy selection method.
    /// </summary>
    public sealed class StimSelectionOptions
    {
        /// <summary>Number of lowest-entropy stimuli to sample from.</summary>
        public int N { get; set; } = 4;

        /// <summary>
        /// Maximum number of times the same stimulus can be selected consecutively
        /// before MinNEntropy reshuffles. Use 0 to disable.
        /// </summary>
        public int MaxConsecutiveReps { get; set; } = 2;

        /// <summary>
        /// Random seed for the RNG used by MinNEntropy. Null means use a
        /// time-based seed (non-deterministic). Set to a fixed integer for
        /// reproducible runs.
        /// </summary>
        public int? RandomSeed { get; set; } = null;
    }
}

namespace BGC.Parameters.Algorithms
{
    /// <summary>
    /// Provides an interface for algorithms that operate on a per-block basis instead of a per-trial basis.
    /// </summary>
    public interface IBlockOutcomeAlgorithm : IAlgorithm, IPropertyGroup
    {
        void Initialize();

        /// <summary>Performs step up/step down behavior based on block results.</summary>
        /// <param name="numTrials">Number of trials in the previous block.</param>
        /// <param name="numTrialsCorrect">Number of trials participant got correct in previous block.</param>
        /// <returns>The participant's performance on the previous block (0 - 1)</returns>
        public double SubmitBlockResults(
            int numTrials,
            int numTrialsCorrect);
    }
}
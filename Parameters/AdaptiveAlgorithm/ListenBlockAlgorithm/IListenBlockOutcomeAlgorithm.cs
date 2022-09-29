namespace BGC.Parameters.Algorithms
{
    public interface IListenBlockOutcomeAlgorithm : IAlgorithm, IPropertyGroup
    {
        void Initialize();

        public void SubmitBlockResults(
            int trialsPerBlock,
            int trialCorrectCount,
            out double performance);
    }
}
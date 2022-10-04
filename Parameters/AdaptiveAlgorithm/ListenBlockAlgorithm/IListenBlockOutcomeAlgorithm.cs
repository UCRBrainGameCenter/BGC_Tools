namespace BGC.Parameters.Algorithms
{
    public interface IListenBlockOutcomeAlgorithm : IAlgorithm, IPropertyGroup
    {
        void Initialize();

        public int SubmitBlockResults(
            int trialsPerBlock,
            int trialCorrectCount,
            out double performance);
    }
}
namespace BGC.Parameters.Algorithms.BlockwiseStaircase
{
    [PropertyGroupTitle("Termination Rule")]
    public interface ITerminationRule : IPropertyGroup
    {
        bool IsDone(int trials, int blocks, int reversals);
    }

    [PropertyChoiceTitle("Total Reversal Count")]
    [IntFieldDisplay("Value", displayTitle: "Reversal Count", initial: 12, minimum: 1, maximum: 10_000, postfix: "reversals")]
    public class ReversalCountTermination : SimpleValueStore<int>, ITerminationRule
    {
        bool ITerminationRule.IsDone(int trials, int blocks, int reversals) => reversals >= Value;
    }

    [PropertyChoiceTitle("Trial Count")]
    [IntFieldDisplay("Value", displayTitle: "Trial Count", initial: 40, minimum: 1, maximum: 10_000, postfix: "trials")]
    public class TrialCountTermination : SimpleValueStore<int>, ITerminationRule
    {
        bool ITerminationRule.IsDone(int trials, int blocks, int reversals) => trials >= Value;
    }

    [PropertyChoiceTitle("Block Count")]
    [IntFieldDisplay("Value", displayTitle: "Trial Count", initial: 10, minimum: 1, maximum: 10_000, postfix: "blocks")]
    public class BlockCountTermination : SimpleValueStore<int>, ITerminationRule
    {
        bool ITerminationRule.IsDone(int trials, int blocks, int reversals) => blocks >= Value;
    }

    [PropertyChoiceTitle("Test Duration")]
    [DoubleFieldDisplay("Value", displayTitle: "Duration", initial: 5, minimum: 0, maximum: 10_000, postfix: "minutes")]
    public class TestDurationTermination : SimpleValueStore<double>, ITerminationRule
    {
        private double endTime = 0.0;

        bool ITerminationRule.IsDone(int trials, int blocks, int reversals) => UnityEngine.Time.time >= endTime;

        public override void InitiatePhase(GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Task:
                    endTime = UnityEngine.Time.time + 60.0 * Value;
                    break;

                case GenerationPhase.Instance:
                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    // Do Nothing
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unsupported GenerationPhase: {phase}");
                    break;
            }
        }
    }
}

namespace BGC.Parameters.Algorithms.SimpleStaircase
{
    [PropertyGroupTitle("Termination Rule")]
    public interface ITerminationRule : IPropertyGroup
    {
        bool IsDone(int trials, int reversals);
    }

    [PropertyChoiceTitle("Total Reversal Count")]
    [IntFieldDisplay("Value", displayTitle: "Reversal Count", initial: 12, minimum: 1, maximum: 10_000, postfix: "reversals")]
    public class ReversalCountTermination : SimpleValueStore<int>, ITerminationRule
    {
        bool ITerminationRule.IsDone(int trials, int reversals) => reversals >= Value;
    }

    [PropertyChoiceTitle("Trial Count")]
    [IntFieldDisplay("Value", displayTitle: "Trial Count", initial: 40, minimum: 1, maximum: 10_000, postfix: "trials")]
    public class TrialCountTermination : SimpleValueStore<int>, ITerminationRule
    {
        bool ITerminationRule.IsDone(int trials, int reversals) => trials >= Value;
    }

    [PropertyChoiceTitle("Test Duration")]
    [DoubleFieldDisplay("Value", displayTitle: "Duration", initial: 5, minimum: 0, maximum: 10_000, postfix: "minutes")]
    public class TestDurationTermination : SimpleValueStore<double>, ITerminationRule
    {
        private double endTime = 0.0;

        bool ITerminationRule.IsDone(int trials, int reversals) => UnityEngine.Time.time >= endTime;

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

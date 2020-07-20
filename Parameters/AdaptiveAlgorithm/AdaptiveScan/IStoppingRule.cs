namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyGroupTitle("Additional Stopping Rule")]
    public interface IStoppingRule : IPropertyGroup
    {
        bool IsDone(int totalScans);
    }

    [PropertyChoiceTitle("Total Scans")]
    [IntFieldDisplay("Value", displayTitle: "Total Scans", initial: 4, minimum: 1, maximum: 10_000, postfix: "scans")]
    public class TotalScansStoppingRule : SimpleValueStore<int>, IStoppingRule
    {
        bool IStoppingRule.IsDone(int totalScans) => totalScans >= Value;
    }

    [PropertyChoiceTitle("None")]
    public class NoAdditionalStoppingRule : StimulusPropertyGroup, IStoppingRule
    {
        bool IStoppingRule.IsDone(int totalScans) => false;
    }

    [PropertyChoiceTitle("Test Duration")]
    [DoubleFieldDisplay("Value", displayTitle: "Duration", initial: 5.0, minimum: 0.0, maximum: 10_000.0, postfix: "minutes")]
    public class TestDurationStoppingRule : SimpleValueStore<double>, IStoppingRule
    {
        private double endTime = 0.0;

        bool IStoppingRule.IsDone(int totalScans) => UnityEngine.Time.time >= endTime;

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

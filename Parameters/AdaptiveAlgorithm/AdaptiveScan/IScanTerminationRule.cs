namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyGroupTitle("Scan Termination Rule")]
    public interface IScanTerminationRule : IPropertyGroup
    {
        bool IsDone(int errorCount);
    }

    [PropertyChoiceTitle("None")]
    public class NoScanTerminationRule : StimulusPropertyGroup, IScanTerminationRule
    {
        bool IScanTerminationRule.IsDone(int errorCount) => false;
    }

    [PropertyChoiceTitle("Error Count")]
    [IntFieldDisplay("Value", displayTitle: "Error Count", initial: 3, minimum: 1, maximum: 10_000, postfix: "errors")]
    public class ErrorCountScanTerminationRule : SimpleValueStore<int>, IScanTerminationRule
    {
        bool IScanTerminationRule.IsDone(int errorCount) => errorCount >= Value;
    }
}

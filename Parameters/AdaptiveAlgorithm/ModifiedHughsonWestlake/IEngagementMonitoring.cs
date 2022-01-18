namespace BGC.Parameters.Algorithms.ModifiedHughsonWestlake
{
    [PropertyGroupTitle("Engagement Monitoring")]
    public interface IEngagementMonitoring : IPropertyGroup
    {
        bool IsLapseDetected(int step);
        void MarkHit(int step);
    }


    [PropertyChoiceTitle("Disabled")]
    public class DisabledEngagementMonitoring : StimulusPropertyGroup, IEngagementMonitoring
    {
        bool IEngagementMonitoring.IsLapseDetected(int step) => false;
        void IEngagementMonitoring.MarkHit(int step) { }
    }

    [PropertyChoiceTitle("Lapse")]
    [IntFieldDisplay("Value", displayTitle: "Steps Above Last Hit", initial: 8, minimum: 1, maximum: 10_000, postfix: "steps")]
    public class LapseMonitoring : SimpleValueStore<int>, IEngagementMonitoring
    {
        private int lastHit = 0;
        bool IEngagementMonitoring.IsLapseDetected(int step) => step < lastHit - Value;
        void IEngagementMonitoring.MarkHit(int step) => lastHit = step;
    }
}

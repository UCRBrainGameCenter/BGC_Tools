using System;

namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyGroupTitle("Out-Of-Bounds Behavior")]
    public interface IOutOfBoundsBehavior : IPropertyGroup
    {
        int MinimumSteps { get; }
    }

    [PropertyChoiceTitle("Clamp")]
    public class ClampOutOfBoundsBehavior : CommonPropertyGroup, IOutOfBoundsBehavior
    {
        public int MinimumSteps => 1;
    }

    [PropertyChoiceTitle("Repeat")]
    public class RepeatOutOfBoundsBehavior : CommonPropertyGroup, IOutOfBoundsBehavior
    {
        public int MinimumSteps => 1;
    }

    [PropertyChoiceTitle("Truncate")]
    [IntFieldDisplay("Value", displayTitle: "Minimum Steps (Or Terminate)", initial: 1, minimum: 1)]
    public class TruncateOutOfBoundsBehavior : SimpleValueStore<int>, IOutOfBoundsBehavior
    {
        public int MinimumSteps => Value;
    }
}
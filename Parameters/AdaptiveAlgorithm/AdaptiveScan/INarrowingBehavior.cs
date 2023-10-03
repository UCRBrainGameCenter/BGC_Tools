using System;

namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyGroupTitle("Narrowing Behavior")]
    public interface INarrowingBehavior : IPropertyGroup
    {
        int Narrow(int val);
    }

    [PropertyChoiceTitle("Scalar")]
    [DoubleFieldDisplay("Value", displayTitle: "Scale", initial: 0.5, minimum: 0.0, maximum: 1.0)]
    public class ScalarNarrowBehavior : SimpleValueStore<double>, INarrowingBehavior
    {
        int INarrowingBehavior.Narrow(int val)
        {
            return (int)Math.Floor(val * Value);
        }
    }

    [PropertyChoiceTitle("Difference")]
    [IntFieldDisplay("Value", displayTitle: "Step Amount", initial: 1, minimum: 0)]
    public class DifferenceNarrowBehavior : SimpleValueStore<int>, INarrowingBehavior
    {
        int INarrowingBehavior.Narrow(int val)
        {
            return Math.Max(val - Value, 0);
        }
    }
}

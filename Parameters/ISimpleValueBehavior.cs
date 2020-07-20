namespace BGC.Parameters
{
    [PropertyGroupTitle("Value Behavior")]
    public interface ISimpleValueBehavior<T> : IPropertyGroup
    {
        T GetValue();
    }

    [PropertyChoiceTitle("Fixed")]
    [FieldMirrorDisplay(fieldName: "Value", mirroredFieldName: "Value", displayTitle: "Value")]
    public class FixedSimpleValueBehavior<T> : SimpleValueStore<T>, ISimpleValueBehavior<T>
    {
        T ISimpleValueBehavior<T>.GetValue() => Value;
    }

    [PropertyChoiceTitle("Roving")]
    [FieldMirrorDisplay(fieldName: "GenerationFrequency", mirroredFieldName: "GenerationFrequency", displayTitle: "Generation Frequency")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadius", mirroredFieldName: "RovingRadius", displayTitle: "Roving Radius")]
    [FieldMirrorDisplay(fieldName: "RovingRadialFactor", mirroredFieldName: "RovingRadialFactor", displayTitle: "Roving Radial Factor")]
    public class RovingSimpleDoubleValueBehavior : RandomizingDoubleProperty, IValueBehavior<double>, ISimpleValueBehavior<double>
    {
        double ISimpleValueBehavior<double>.GetValue() => DoubleValue;
        double IValueBehavior<double>.GetValue(bool target) => DoubleValue;
    }

    [PropertyGroupTitle("Value Behavior")]
    public interface IValueBehavior<T> : IPropertyGroup
    {
        T GetValue(bool target);
    }

    [PropertyChoiceTitle("Fixed")]
    [FieldMirrorDisplay(fieldName: "Value", mirroredFieldName: "Value", displayTitle: "Value")]
    public class FixedIntValueBehavior : SimpleIntValueStore, IValueBehavior<int>
    {
        int IValueBehavior<int>.GetValue(bool target) => GetValue(target);
    }

    [PropertyChoiceTitle("Roving")]
    [FieldMirrorDisplay(fieldName: "GenerationFrequency", mirroredFieldName: "GenerationFrequency", displayTitle: "Generation Frequency")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadius", mirroredFieldName: "RovingRadius", displayTitle: "Roving Radius")]
    public class RovingIntValueBehavior : RandomizingIntProperty, IValueBehavior<int>, ISimpleValueBehavior<int>
    {
        int IValueBehavior<int>.GetValue(bool target) => IntValue;
        int ISimpleValueBehavior<int>.GetValue() => IntValue;
    }
}


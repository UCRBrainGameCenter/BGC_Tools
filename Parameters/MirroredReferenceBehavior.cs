namespace BGC.Parameters
{
    [PropertyChoiceTitle("Mirrored")]
    public class MirroredStandardBehavior<T> : StimulusPropertyGroup, IStandardBehavior<T>
    {
        T IStandardBehavior<T>.GetStandard(T targetValue) => targetValue;
    }

    [PropertyChoiceTitle("Fixed")]
    [FieldMirrorDisplay(fieldName: "StandardValue", mirroredFieldName: "StandardValue", displayTitle: "Standard Value")]
    public class SplitStandardBehavior<T> : StimulusPropertyGroup, IStandardBehavior<T>
    {
        [DisplayInputField("StandardValue")]
        public T StandardValue { get; set; }

        [DisplayInputFieldKey("StandardValue")]
        public string StandardValueKey { get; set; }

        T IStandardBehavior<T>.GetStandard(T targetValue) => StandardValue;
    }
}

using System.Threading;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Roving")]
    public abstract class RandomizingIntProperty : RandomizingStimulusPropertyGroup
    {
        [DisplayInputField("GenerationFrequency")]
        public GenerationPhase GenerationFrequency { get; set; }
        [AppendSelection(
            typeof(RadialIntRove),
            typeof(RangeIntRove))]
        public IRovingIntBehavior RovingBehavior { get; set; }

        protected virtual int GetRandomValue() => RovingBehavior.GetRandomValue(Randomizer);
        protected int UpperBound => RovingBehavior.UpperBound;
        protected int LowerBound => RovingBehavior.LowerBound;

        private int _intValue;
        private ThreadLocal<int> _threadLocalIntValue = new ThreadLocal<int>(() => 0);
        protected int IntValue
        {
            get
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance: return GetRandomValue();
                    case GenerationPhase.Interval: return _threadLocalIntValue.Value;
                    case GenerationPhase.Trial:
                    case GenerationPhase.Task: return _intValue;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        return _intValue;
                }
            }

            set
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance:
                        //Do Nothing
                        break;

                    case GenerationPhase.Interval:
                        _threadLocalIntValue.Value = value;
                        break;

                    case GenerationPhase.Trial:
                    case GenerationPhase.Task:
                        _intValue = value;
                        break;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        _intValue = value;
                        break;
                }
            }
        }

        public override void InitiatePhase(GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance:
                    //Do Nothing
                    break;

                case GenerationPhase.Task:
                    IntValue = GetRandomValue();
                    break;

                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    if (phase == GenerationFrequency)
                    {
                        IntValue = GetRandomValue();
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase value: {phase}");
                    break;
            }
        }
    }

    [PropertyGroupTitle("Roving Behavior")]
    public interface IRovingIntBehavior : IPropertyGroup
    {
        int GetRandomValue(System.Random Randomizer);
        int LowerBound { get; }
        int UpperBound { get; }
    }

    [PropertyChoiceTitle("Linear Radial Rove")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadius", mirroredFieldName: "RovingRadius", displayTitle: "Roving Radius")]
    public class RadialIntRove : StimulusPropertyGroup, IRovingIntBehavior
    {
        [DisplayInputField("CentralValue")]
        public int CentralValue { get; set; }
        [DisplayInputField("RovingRadius")]
        public int RovingRadius { get; set; }

        [DisplayInputFieldKey("CentralValue")]
        public string CentralValueKey { get; set; }
        [DisplayInputFieldKey("RovingRadius")]
        public string RovingRadiusKey { get; set; }

        int IRovingIntBehavior.GetRandomValue(System.Random Randomizer) => Randomizer.Next(CentralValue - RovingRadius, CentralValue + RovingRadius + 1);
        int IRovingIntBehavior.LowerBound => CentralValue - RovingRadius;
        int IRovingIntBehavior.UpperBound => CentralValue + RovingRadius;
    }

    [PropertyChoiceTitle("Linear Range Rove")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    public class RangeIntRove : StimulusPropertyGroup, IRovingIntBehavior
    {
        [DisplayInputField("LowerBound")]
        public int LowerBound { get; set; }
        [DisplayInputField("UpperBound")]
        public int UpperBound { get; set; }

        [DisplayInputFieldKey("LowerBound")]
        public string LowerBoundKey { get; set; }
        [DisplayInputFieldKey("UpperBound")]
        public string UpperBoundKey { get; set; }

        int IRovingIntBehavior.GetRandomValue(System.Random Randomizer) => Randomizer.Next(LowerBound, UpperBound + 1);
        int IRovingIntBehavior.LowerBound => LowerBound;
        int IRovingIntBehavior.UpperBound => UpperBound;
    }
}

using System;
using System.Threading;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Roving")]
    public abstract class RandomizingDoubleProperty : RandomizingStimulusPropertyGroup
    {
        [DisplayInputField("GenerationFrequency")]
        public GenerationPhase GenerationFrequency { get; set; }

        [AppendSelection(
            typeof(LinearRadialRove),
            typeof(LinearRangeRove),
            typeof(ExponentialRadialRove),
            typeof(ExponentialRangeRove))]
        public IRovingDoubleBehavior RovingBehavior { get; set; }

        protected virtual double GetRandomValue() => RovingBehavior.GetRandomValue(Randomizer);
        protected double UpperBound => RovingBehavior.UpperBound;
        protected double LowerBound => RovingBehavior.LowerBound;

        private double _doubleValue;
        private ThreadLocal<double> _threadLocalDoubleValue = new ThreadLocal<double>(() => 0.0);
        protected double DoubleValue
        {
            get
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance: return GetRandomValue();
                    case GenerationPhase.Interval: return _threadLocalDoubleValue.Value;
                    case GenerationPhase.Trial:
                    case GenerationPhase.Task: return _doubleValue;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        return _doubleValue;
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
                        _threadLocalDoubleValue.Value = value;
                        break;

                    case GenerationPhase.Trial:
                    case GenerationPhase.Task:
                        _doubleValue = value;
                        break;

                    case GenerationPhase.MAX:
                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        _doubleValue = value;
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
                    DoubleValue = GetRandomValue();
                    break;

                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    if (phase == GenerationFrequency)
                    {
                        DoubleValue = GetRandomValue();
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase value: {phase}");
                    break;
            }
        }
    }

    [PropertyChoiceTitle("Roving")]
    public abstract class RandomizingLinearDoubleProperty : RandomizingStimulusPropertyGroup
    {
        [DisplayInputField("GenerationFrequency")]
        public GenerationPhase GenerationFrequency { get; set; }

        [AppendSelection(
            typeof(LinearRadialRove),
            typeof(LinearRangeRove))]
        public IRovingDoubleBehavior RovingBehavior { get; set; }

        protected virtual double GetRandomValue() => RovingBehavior.GetRandomValue(Randomizer);
        protected double UpperBound => RovingBehavior.UpperBound;
        protected double LowerBound => RovingBehavior.LowerBound;

        private double _doubleValue;
        private ThreadLocal<double> _threadLocalDoubleValue = new ThreadLocal<double>(() => 0.0);
        protected double DoubleValue
        {
            get
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance: return GetRandomValue();
                    case GenerationPhase.Interval: return _threadLocalDoubleValue.Value;
                    case GenerationPhase.Trial:
                    case GenerationPhase.Task: return _doubleValue;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        return _doubleValue;
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
                        _threadLocalDoubleValue.Value = value;
                        break;

                    case GenerationPhase.Trial:
                    case GenerationPhase.Task:
                        _doubleValue = value;
                        break;

                    case GenerationPhase.MAX:
                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        _doubleValue = value;
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
                    DoubleValue = GetRandomValue();
                    break;

                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    if (phase == GenerationFrequency)
                    {
                        DoubleValue = GetRandomValue();
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase value: {phase}");
                    break;
            }
        }
    }

    [PropertyChoiceTitle("Linear Radial Rove")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadius", mirroredFieldName: "RovingRadius", displayTitle: "Roving Radius")]
    public class LinearRadialRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("CentralValue")]
        public double CentralValue { get; set; }
        [DisplayInputField("RovingRadius")]
        public double RovingRadius { get; set; }

        [DisplayInputFieldKey("CentralValue")]
        public string CentralValueKey { get; set; }
        [DisplayInputFieldKey("RovingRadius")]
        public string RovingRadiusKey { get; set; }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            CentralValue + (RovingRadius * (2.0 * Randomizer.NextDouble() - 1.0));

        double IRovingDoubleBehavior.LowerBound => CentralValue - RovingRadius;
        double IRovingDoubleBehavior.UpperBound => CentralValue + RovingRadius;
    }

    [PropertyChoiceTitle("Linear Range Rove")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    public class LinearRangeRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("LowerBound")]
        public double LowerBound { get; set; }
        [DisplayInputField("UpperBound")]
        public double UpperBound { get; set; }

        [DisplayInputFieldKey("LowerBound")]
        public string LowerBoundKey { get; set; }
        [DisplayInputFieldKey("UpperBound")]
        public string UpperBoundKey { get; set; }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound + ((UpperBound - LowerBound) * Randomizer.NextDouble());
        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyChoiceTitle("Exponential Radial Rove")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadialFactor", mirroredFieldName: "RovingRadialFactor", displayTitle: "Roving Radial Factor")]
    public class ExponentialRadialRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("CentralValue")]
        public double CentralValue { get; set; }
        [DisplayInputField("RovingRadialFactor")]
        public double RovingRadialFactor { get; set; }

        [DisplayInputFieldKey("CentralValue")]
        public string CentralValueKey { get; set; }
        [DisplayInputFieldKey("RovingRadialFactor")]
        public string RovingRadialFactorKey { get; set; }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound * Math.Pow((UpperBound / LowerBound), Randomizer.NextDouble());

        private double LowerBound => CentralValue / RovingRadialFactor;
        private double UpperBound => CentralValue * RovingRadialFactor;

        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyChoiceTitle("Exponential Range Rove")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    public class ExponentialRangeRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("LowerBound")]
        public double LowerBound { get; set; }
        [DisplayInputField("UpperBound")]
        public double UpperBound { get; set; }

        [DisplayInputFieldKey("LowerBound")]
        public string LowerBoundKey { get; set; }
        [DisplayInputFieldKey("UpperBound")]
        public string UpperBoundKey { get; set; }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound * Math.Pow((UpperBound / LowerBound), Randomizer.NextDouble());
        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyGroupTitle("Roving Behavior")]
    public interface IRovingDoubleBehavior : IPropertyGroup
    {
        double GetRandomValue(System.Random Randomizer);
        double LowerBound { get; }
        double UpperBound { get; }
    }
}

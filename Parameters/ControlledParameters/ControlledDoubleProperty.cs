using BGC.Parameters.Algorithms;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Controlled", renderingModifier: ChoiceRenderingModifier.Controlled)]
    public abstract class ControlledDoubleProperty : StimulusPropertyGroup, IControlled
    {
        public double GetValue(bool target) => target ? value : Standard;
        public double value;
        public double Standard => StandardBehavior.GetStandard(value);

        [AppendSelection(
            typeof(MirroredStandardBehavior<double>),
            typeof(SplitStandardBehavior<double>))]
        public IStandardBehavior<double> StandardBehavior { get; set; }

        #region IControlled

        StepStatus IControlled.StepTo(int stepNumber, ControlledParameterTemplate template)
        {
            if (template is IDoubleParameterTemplate doubleTemplate)
            {
                value = doubleTemplate.GetValue(stepNumber);
                return StepStatus.Success;
            }

            return StepStatus.TypeError;
        }

        double IControlled.GetPartialStepValue(double stepValue, ControlledParameterTemplate template)
        {
            if (template is IDoubleParameterTemplate doubleTemplate)
            {
                return doubleTemplate.GetPartialValue(stepValue);
            }

            UnityEngine.Debug.LogError("Mismatched type.");
            return double.NaN;
        }

        ControlledBasis IControlled.ControlledBasis => ControlledBasis.FloatingPoint;
        string IControlled.GetValueString() => value.ToString();

        #endregion IControlled
    }

    [PropertyChoiceTitle("Controlled", renderingModifier: ChoiceRenderingModifier.Controlled)]
    public abstract class ControlledSimpleDoubleProperty : StimulusPropertyGroup, IControlled
    {
        public double GetValue() => value;
        public double value;

        #region IControlled

        StepStatus IControlled.StepTo(int stepNumber, ControlledParameterTemplate template)
        {
            if (template is IDoubleParameterTemplate doubleTemplate)
            {
                value = doubleTemplate.GetValue(stepNumber);
                return StepStatus.Success;
            }

            return StepStatus.TypeError;
        }

        double IControlled.GetPartialStepValue(double stepValue, ControlledParameterTemplate template)
        {
            if (template is IDoubleParameterTemplate doubleTemplate)
            {
                return doubleTemplate.GetPartialValue(stepValue);
            }

            UnityEngine.Debug.LogError("Mismatched type.");
            return double.NaN;
        }

        ControlledBasis IControlled.ControlledBasis => ControlledBasis.FloatingPoint;
        string IControlled.GetValueString() => value.ToString();

        #endregion IControlled
    }
}

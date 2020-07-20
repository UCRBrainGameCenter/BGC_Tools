using BGC.Mathematics;
using BGC.Parameters.Algorithms;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Controlled", renderingModifier: ChoiceRenderingModifier.Controlled)]
    public abstract class ControlledIntProperty : StimulusPropertyGroup, IControlled
    {
        public int GetValue(bool target) => target ? value : Standard;
        public int value;
        public int Standard => StandardBehavior.GetStandard(value);

        [AppendSelection(
            typeof(MirroredStandardBehavior<int>),
            typeof(SplitStandardBehavior<int>))]
        public IStandardBehavior<int> StandardBehavior { get; set; }

        #region IControlled

        StepStatus IControlled.StepTo(int stepNumber, ControlledParameterTemplate template)
        {
            if (template is IIntParameterTemplate intTemplate)
            {
                value = intTemplate.GetValue(stepNumber);
                return StepStatus.Success;
            }

            return StepStatus.TypeError;
        }

        double IControlled.GetPartialStepValue(double stepValue, ControlledParameterTemplate template)
        {
            if (template is IIntParameterTemplate intTemplate)
            {
                return intTemplate.GetPartialValue(stepValue);
            }

            UnityEngine.Debug.LogError("Mismatched type.");
            return double.NaN;
        }

        ControlledBasis IControlled.ControlledBasis => ControlledBasis.Integer;
        string IControlled.GetValueString() => value.ToString();

        #endregion IControlled
    }

    [PropertyChoiceTitle("Controlled", renderingModifier: ChoiceRenderingModifier.Controlled)]
    public abstract class ControlledSimpleIntProperty : StimulusPropertyGroup, IControlled
    {
        public int GetValue() => value;
        public int value;

        #region IControlled

        StepStatus IControlled.StepTo(int stepNumber, ControlledParameterTemplate template)
        {
            if (template is IIntParameterTemplate intTemplate)
            {
                value = intTemplate.GetValue(stepNumber);
                return StepStatus.Success;
            }

            return StepStatus.TypeError;
        }

        double IControlled.GetPartialStepValue(double stepValue, ControlledParameterTemplate template)
        {
            if (template is IIntParameterTemplate intTemplate)
            {
                return intTemplate.GetPartialValue(stepValue);
            }

            UnityEngine.Debug.LogError("Mismatched type.");
            return double.NaN;
        }

        ControlledBasis IControlled.ControlledBasis => ControlledBasis.Integer;
        string IControlled.GetValueString() => value.ToString();

        #endregion IControlled
    }

}

using System;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Controlled", renderingModifier: ChoiceRenderingModifier.Controlled)]
    public abstract class ControlledStringProperty : StimulusPropertyGroup, IControlled
    {
        public string GetValue(bool target) => target ? value : Standard;
        public string value;
        public string Standard => StandardBehavior.GetStandard(value);

        [AppendSelection(
            typeof(MirroredStandardBehavior<string>),
            typeof(SplitStandardBehavior<string>))]
        public IStandardBehavior<string> StandardBehavior { get; set; }

        #region IControlled

        StepStatus IControlled.StepTo(int stepNumber, ControlledParameterTemplate template)
        {
            if (template is IStringParameterTemplate stringTemplate)
            {
                value = stringTemplate.GetValue(stepNumber);
                return StepStatus.Success;
            }

            return StepStatus.TypeError;
        }

        double IControlled.GetPartialStepValue(double stepValue, ControlledParameterTemplate template) => throw new NotImplementedException();

        ControlledBasis IControlled.ControlledBasis => ControlledBasis.String;
        string IControlled.GetValueString() => value.ToString();

        #endregion IControlled
    }

}

using System;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Linear")]
    [FieldMirrorDisplay("BaseValue", mirroredFieldName: "BaseValue", displayTitle: "Base Value")]
    [FieldMirrorDisplay("Min", mirroredFieldName: "Min", displayTitle: "Minimum")]
    [FieldMirrorDisplay("Max", mirroredFieldName: "Max", displayTitle: "Maximum")]
    [FieldMirrorDisplay("BaseStepSize", mirroredFieldName: "BaseStepSize", displayTitle: "Step Size")]
    [BoolDisplay("DecreaseParameter", displayTitle: "Decrease Value On Step Down", initial: true)]
    public class SimpleIntLinearSteps : StimulusPropertyGroup, ISimpleIntStepTemplate
    {
        [DisplayInputField("BaseValue")]
        public int BaseValue { get; set; }
        [DisplayInputField("Min")]
        public int Minimum { get; set; }
        [DisplayInputField("Max")]
        public int Maximum { get; set; }

        [DisplayInputField("DecreaseParameter")]
        public bool DecreaseParameter { get; set; }

        [DisplayInputFieldKey("BaseValue")]
        public string BaseValueKey { get; set; }
        [DisplayInputFieldKey("Min")]
        public string MinimumKey { get; set; }
        [DisplayInputFieldKey("Max")]
        public string MaximumKey { get; set; }

        [DisplayInputField("BaseStepSize")]
        public int BaseStepSize { get; set; }

        int ISimpleIntStepTemplate.GetValue(int stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(BaseValue + BaseStepSize * stepNumber, Minimum, Maximum);
        }

        double ISimpleIntStepTemplate.GetPartialValue(double stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(BaseValue + BaseStepSize * stepNumber, Minimum, Maximum);
        }

        void ISimpleIntStepTemplate.Initialize() { }


        bool ISimpleIntStepTemplate.CouldStepTo(int stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            int potentialValue = BaseValue + BaseStepSize * stepNumber;
            return potentialValue >= Minimum && potentialValue <= Maximum;
        }
    }
}

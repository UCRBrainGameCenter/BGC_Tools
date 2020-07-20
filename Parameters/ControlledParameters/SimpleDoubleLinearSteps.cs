using System;
using BGC.Mathematics;
using BGC.Scripting;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Linear")]
    [FieldMirrorDisplay("BaseValue", mirroredFieldName: "BaseValue", displayTitle: "Base Value")]
    [FieldMirrorDisplay("Min", mirroredFieldName: "Min", displayTitle: "Minimum")]
    [FieldMirrorDisplay("Max", mirroredFieldName: "Max", displayTitle: "Maximum")]
    [FieldMirrorDisplay("BaseStepSize", mirroredFieldName:"BaseStepSize", displayTitle:"Step Size")]
    [BoolDisplay("DecreaseParameter", displayTitle: "Decrease Value On Step Down", initial: true)]
    public class SimpleDoubleLinearSteps : StimulusPropertyGroup, ISimpleDoubleStepTemplate
    {
        [DisplayInputField("BaseValue")]
        public double BaseValue { get; set; }
        [DisplayInputField("Min")]
        public double Minimum { get; set; }
        [DisplayInputField("Max")]
        public double Maximum { get; set; }

        [DisplayInputField("DecreaseParameter")]
        public bool DecreaseParameter { get; set; }

        [DisplayInputFieldKey("BaseValue")]
        public string BaseValueKey { get; set; }
        [DisplayInputFieldKey("Min")]
        public string MinimumKey { get; set; }
        [DisplayInputFieldKey("Max")]
        public string MaximumKey { get; set; }

        [DisplayInputField("BaseStepSize")]
        public double BaseStepSize { get; set; }

        double ISimpleDoubleStepTemplate.GetValue(int stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(BaseValue + BaseStepSize * stepNumber, Minimum, Maximum);
        }

        double ISimpleDoubleStepTemplate.GetPartialValue(double stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(BaseValue + BaseStepSize * stepNumber, Minimum, Maximum);
        }

        void ISimpleDoubleStepTemplate.Initialize() { }

        bool ISimpleDoubleStepTemplate.CouldStepTo(int stepNumber)
        {
            if (DecreaseParameter)
            {
                stepNumber *= -1;
            }

            double potentialValue = BaseValue + BaseStepSize * stepNumber;
            return potentialValue >= Minimum && potentialValue <= Maximum;
        }
    }
}

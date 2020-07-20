using System;
using BGC.Mathematics;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Exponential")]
    [FieldMirrorDisplay("BaseValue", mirroredFieldName: "BaseValue", displayTitle: "Base Value")]
    [FieldMirrorDisplay("ConvergenceValue", mirroredFieldName: "ConvergenceValue", displayTitle: "Convergence Value")]
    [FieldMirrorDisplay("Min", mirroredFieldName: "Min", displayTitle: "Minimum")]
    [FieldMirrorDisplay("Max", mirroredFieldName: "Max", displayTitle: "Maximum")]
    [FieldMirrorDisplay("BaseMajorFactor", mirroredFieldName: "BaseMajorFactor", displayTitle:"Major Factor")]
    [FieldMirrorDisplay("StepsToMajorFactor", mirroredFieldName: "StepsToMajorFactor", displayTitle: "Steps To Major Factor")]
    [BoolDisplay("DecreaseParameter", displayTitle: "Decrease Value On Step Down", initial: true)]
    public class SimpleDoubleExponentialSteps : StimulusPropertyGroup, ISimpleDoubleStepTemplate
    {
        [DisplayInputField("BaseValue")]
        public double BaseValue { get; set; }
        [DisplayInputField("Min")]
        public double Minimum { get; set; }
        [DisplayInputField("Max")]
        public double Maximum { get; set; }
        [DisplayInputField("ConvergenceValue")]
        public double ConvergenceValue { get; set; }

        [DisplayInputField("DecreaseParameter")]
        public bool DecreaseParameter { get; set; }

        [DisplayInputFieldKey("BaseValue")]
        public string BaseValueKey { get; set; }
        [DisplayInputFieldKey("Min")]
        public string MinimumKey { get; set; }
        [DisplayInputFieldKey("Max")]
        public string MaximumKey { get; set; }
        [DisplayInputFieldKey("ConvergenceValue")]
        public string ConvergenceKey { get; set; }

        [DisplayInputField("BaseMajorFactor")]
        public double BaseMajorFactor { get; set; }
        [DisplayInputField("StepsToMajorFactor")]
        public double StepsPerMajorFactor { get; set; }

        [DisplayInputFieldKey("BaseMajorFactor")]
        public string BaseMajorFactorKey { get; set; }
        [DisplayInputFieldKey("StepsToMajorFactor")]
        public string StepsPerMajorFactorKey { get; set; }

        private double delta;
        private bool flipSign;

        double ISimpleDoubleStepTemplate.GetValue(int stepNumber)
        {
            if (flipSign)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(ConvergenceValue + delta * Math.Pow(BaseMajorFactor, stepNumber / StepsPerMajorFactor), Minimum, Maximum);
        }

        double ISimpleDoubleStepTemplate.GetPartialValue(double stepNumber)
        {
            if (flipSign)
            {
                stepNumber *= -1;
            }

            return GeneralMath.Clamp(ConvergenceValue + delta * Math.Pow(BaseMajorFactor, stepNumber / StepsPerMajorFactor), Minimum, Maximum);
        }

        void ISimpleDoubleStepTemplate.Initialize()
        {
            delta = BaseValue - ConvergenceValue;

            if (delta == 0.0)
            {
                throw new ParameterizedCompositionException($"Cannot have a Convergence Value equal to the BaseValue.", this.GetGroupPath());
            }

            //We flip the sign of the stepNumber if DecreaseParameter when Delta is positive,
            //or !DecreaseParameter when Delta is negative.
            flipSign = (DecreaseParameter != delta < 0.0);
        }

        bool ISimpleDoubleStepTemplate.CouldStepTo(int stepNumber)
        {
            if (flipSign)
            {
                stepNumber *= -1;
            }

            double potentialValue = ConvergenceValue + delta * Math.Pow(BaseMajorFactor, stepNumber / StepsPerMajorFactor);

            return potentialValue >= Minimum && potentialValue <= Maximum;
        }
    }
}

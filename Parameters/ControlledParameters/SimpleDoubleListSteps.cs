using System;
using System.Linq;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("List")]
    [StringFieldDisplay("ValueList", "Values", "1.0, 2.0, 3.0")]
    public class SimpleDoubleListSteps : StimulusPropertyGroup, ISimpleDoubleStepTemplate
    {
        [DisplayInputField("ValueList")]
        public string ValueList { get; set; }
        [DisplayInputFieldKey("ValueList")]
        public string ValueListKey { get; set; }

        private double[] values = null;

        private static readonly char[] separators = new char[] { ',', ' ', '\n', '\r' };

        double ISimpleDoubleStepTemplate.GetValue(int stepNumber) =>
            values[GeneralMath.Clamp(stepNumber, 0, values.Length - 1)];

        double ISimpleDoubleStepTemplate.GetPartialValue(double stepNumber) =>
            values[(int)Math.Round(GeneralMath.Clamp(stepNumber, 0, values.Length - 1))];

        void ISimpleDoubleStepTemplate.Initialize() =>
            values = ValueList.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();

        bool ISimpleDoubleStepTemplate.CouldStepTo(int stepNumber) =>
            stepNumber >= 0 && stepNumber < values.Length;
    }
}

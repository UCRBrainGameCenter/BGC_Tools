using System;
using System.Linq;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("List")]
    [StringFieldDisplay("ValueList", "Values", "1, 2, 3")]
    public class SimpleIntListSteps : StimulusPropertyGroup, ISimpleIntStepTemplate
    {
        [DisplayInputField("ValueList")]
        public string ValueList { get; set; }
        [DisplayInputFieldKey("ValueList")]
        public string ValueListKey { get; set; }

        private int[] values = null;

        private static readonly char[] separators = new char[] { ',', ' ', '\n', '\r' };

        int ISimpleIntStepTemplate.GetValue(int stepNumber) => values[stepNumber];

        double ISimpleIntStepTemplate.GetPartialValue(double stepNumber) =>
            values[(int)Math.Round(GeneralMath.Clamp(stepNumber, 0, values.Length - 1))];

        void ISimpleIntStepTemplate.Initialize() =>
            values = ValueList.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

        bool ISimpleIntStepTemplate.CouldStepTo(int stepNumber) =>
            stepNumber >= 0 && stepNumber < values.Length;
    }
}

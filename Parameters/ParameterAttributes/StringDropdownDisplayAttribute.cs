using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringDropdownDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string choiceListMethodName;
        public readonly string initialValue;
        public readonly bool retainMissingValues;
        public readonly bool forceRefreshOnChange;

        public StringDropdownDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initialValue,
            bool retainMissingValues,
            string choiceListMethodName,
            bool forceRefreshOnChange = false)
            : base(fieldName, displayTitle)
        {
            this.initialValue = initialValue;
            this.retainMissingValues = retainMissingValues;
            this.choiceListMethodName = choiceListMethodName;
            this.forceRefreshOnChange = forceRefreshOnChange;
        }

        public override object GetInitialValue() => initialValue;
    }
}

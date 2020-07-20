using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EnumDropdownDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string choiceListMethodName;
        public readonly int initialValue;

        public EnumDropdownDisplayAttribute(
            string fieldName,
            string displayTitle,
            int initialValue,
            string choiceListMethodName)
            : base(fieldName, displayTitle)
        {
            this.initialValue = initialValue;

            this.choiceListMethodName = choiceListMethodName;
        }

        public override object GetInitialValue() => initialValue;
    }
}

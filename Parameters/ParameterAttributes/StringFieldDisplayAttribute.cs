using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringFieldDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string initial;

        public StringFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initial = "")
            : base(fieldName, displayTitle)
        {
            this.initial = initial;
        }

        public override object GetInitialValue() => initial;
    }
}

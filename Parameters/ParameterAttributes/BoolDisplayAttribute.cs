using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoolDisplayAttribute : FieldDisplayAttribute
    {
        public readonly bool initial;

        public BoolDisplayAttribute(
            string fieldName,
            string displayTitle,
            bool initial = false)
            : base(fieldName, displayTitle)
        {
            this.initial = initial;
        }

        public override object GetInitialValue() => initial;
    }
}

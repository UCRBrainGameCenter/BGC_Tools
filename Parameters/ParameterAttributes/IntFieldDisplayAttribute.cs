using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntFieldDisplayAttribute : FieldDisplayAttribute
    {
        public readonly int initial;
        public readonly int minimum;
        public readonly int maximum;

        public readonly string postfix;

        public IntFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int initial = 0,
            int minimum = int.MinValue,
            int maximum = int.MaxValue,
            string postfix = "")
            : base(fieldName, displayTitle)
        {
            this.initial = initial;
            this.minimum = minimum;
            this.maximum = maximum;

            this.postfix = postfix;
        }

        public override object GetInitialValue() => initial;
    }
}

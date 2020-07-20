using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DoubleFieldDisplayAttribute : FieldDisplayAttribute
    {
        public readonly double initial;
        public readonly double minimum;
        public readonly double maximum;

        public readonly string postfix;

        public DoubleFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            double initial = 0.0,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
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

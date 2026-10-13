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

        /// <summary>
        /// When true, this field is dimensionless (e.g. a multiplicative factor or count)
        /// and will not inherit a unit postfix or min/max constraints from a parent ValueSourceSelection.
        /// </summary>
        public readonly bool unitless;

        public DoubleFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            double initial = 0.0,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
            string postfix = "",
            bool unitless = false)
            : base(fieldName, displayTitle)
        {
            this.initial = initial;
            this.minimum = minimum;
            this.maximum = maximum;
            this.postfix = postfix;
            this.unitless = unitless;
        }

        public override object GetInitialValue() => initial;
    }
}

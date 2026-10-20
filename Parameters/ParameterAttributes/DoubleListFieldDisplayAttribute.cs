using System;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DoubleListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly double[] initial;
        public readonly double minimum;
        public readonly double maximum;
        public readonly double newItem;

        public readonly string postfix;

        public DoubleListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            double[] initial = null,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
            string postfix = "",
            double newItem = 0.0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            char[] legacyDelimiters = null)
            : base(fieldName, displayTitle, minimumCount, maximumCount, legacyDelimiters)
        {
            this.initial = initial ?? Array.Empty<double>();
            this.minimum = minimum;
            this.maximum = maximum;
            this.newItem = newItem;

            this.postfix = postfix;
        }

        public override Type ElementType => typeof(double);

        public override object NewItemValue => Math.Min(Math.Max(newItem, minimum), maximum);

        public override bool IsItemValid(object item) =>
            item is double value && value >= minimum && value <= maximum;

        // A fresh list every call, so property groups never share an instance.
        public override object GetInitialValue() => new List<double>(initial);
    }
}

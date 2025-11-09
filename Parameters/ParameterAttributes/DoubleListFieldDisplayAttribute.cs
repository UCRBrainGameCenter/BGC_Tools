using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DoubleListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly double initial;
        public readonly double minimum;
        public readonly double maximum;
        public readonly string postfix;

        public DoubleListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            double initialValue = 0.0,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
            string postfix = "",
            int initialCount = 0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue)
            : base(fieldName, displayTitle, initialCount, minimumCount, maximumCount)
        {
            initial = initialValue;
            this.minimum = minimum;
            this.maximum = maximum;
            this.postfix = postfix;
        }

        public override Type ElementType => typeof(double);

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(initial);
    }
}

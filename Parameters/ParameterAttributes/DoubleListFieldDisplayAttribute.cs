using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DoubleListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly double defaultValue;
        public readonly double minimum;
        public readonly double maximum;
        public readonly string postfix;
        private readonly double[] initialValues;

        public DoubleListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            double defaultValue = 0.0,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
            string postfix = "",
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            params double[] initialValues)
            : base(fieldName, displayTitle, minimumCount, maximumCount)
        {
            this.defaultValue = defaultValue;
            this.minimum = minimum;
            this.maximum = maximum;
            this.postfix = postfix;
            this.initialValues = initialValues ?? Array.Empty<double>();
        }

        public override Type ElementType => typeof(double);

        protected override void AddInitialValues(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            if (initialValues == null)
            {
                return;
            }

            for (int i = 0; i < initialValues.Length; i++)
            {
                if (IsAtMaximumCount(list))
                {
                    break;
                }

                list.Add(initialValues[i]);
            }
        }

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(defaultValue);
    }
}

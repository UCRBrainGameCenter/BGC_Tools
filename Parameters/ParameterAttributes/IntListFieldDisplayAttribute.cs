using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly int defaultValue;
        public readonly int minimum;
        public readonly int maximum;
        public readonly string postfix;
        private readonly int[] initialValues;

        public IntListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int defaultValue = 0,
            int minimum = int.MinValue,
            int maximum = int.MaxValue,
            string postfix = "",
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            params int[] initialValues)
            : base(fieldName, displayTitle, minimumCount, maximumCount)
        {
            this.defaultValue = defaultValue;
            this.minimum = minimum;
            this.maximum = maximum;
            this.postfix = postfix;
            this.initialValues = initialValues ?? Array.Empty<int>();
        }

        public override Type ElementType => typeof(int);

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

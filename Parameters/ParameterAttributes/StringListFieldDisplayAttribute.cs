using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly string defaultValue;
        private readonly string[] initialValues;

        public StringListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            string defaultValue = "",
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            params string[] initialValues)
            : base(fieldName, displayTitle, minimumCount, maximumCount)
        {
            this.defaultValue = defaultValue ?? string.Empty;
            this.initialValues = initialValues ?? Array.Empty<string>();
        }

        public override Type ElementType => typeof(string);

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

                list.Add(initialValues[i] ?? string.Empty);
            }
        }

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(defaultValue);
    }
}

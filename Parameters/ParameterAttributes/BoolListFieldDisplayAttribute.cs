using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoolListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly bool defaultValue;
        private readonly bool[] initialValues;

        public BoolListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            bool defaultValue = false,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            params bool[] initialValues)
            : base(fieldName, displayTitle, minimumCount, maximumCount)
        {
            this.defaultValue = defaultValue;
            this.initialValues = initialValues ?? Array.Empty<bool>();
        }

        public override Type ElementType => typeof(bool);

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

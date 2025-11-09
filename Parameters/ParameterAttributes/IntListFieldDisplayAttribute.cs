using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly int initial;
        public readonly int minimum;
        public readonly int maximum;
        public readonly string postfix;

        public IntListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int initialValue = 0,
            int minimum = int.MinValue,
            int maximum = int.MaxValue,
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

        public override Type ElementType => typeof(int);

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(initial);
    }
}

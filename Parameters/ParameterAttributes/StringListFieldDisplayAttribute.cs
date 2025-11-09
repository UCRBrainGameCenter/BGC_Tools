using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly string initial;

        public StringListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initialValue = "",
            int initialCount = 0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue)
            : base(fieldName, displayTitle, initialCount, minimumCount, maximumCount)
        {
            initial = initialValue ?? string.Empty;
        }

        public override Type ElementType => typeof(string);

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(initial);
    }
}

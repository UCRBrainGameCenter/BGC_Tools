using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoolListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly bool initial;

        public BoolListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            bool initialValue = false,
            int initialCount = 0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue)
            : base(fieldName, displayTitle, initialCount, minimumCount, maximumCount)
        {
            initial = initialValue;
        }

        public override Type ElementType => typeof(bool);

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(initial);
    }
}

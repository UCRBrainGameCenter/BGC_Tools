using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ListFieldDisplayAttribute : FieldDisplayAttribute
    {
        public int InitialCount { get; }
        public int MinimumCount { get; }
        public int MaximumCount { get; }

        protected ListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int initialCount = 0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue)
            : base(fieldName, displayTitle)
        {
            if (minimumCount < 0)
            {
                minimumCount = 0;
            }

            if (maximumCount < minimumCount)
            {
                maximumCount = minimumCount;
            }

            MinimumCount = minimumCount;
            MaximumCount = maximumCount;

            if (initialCount < MinimumCount)
            {
                initialCount = MinimumCount;
            }
            else if (initialCount > MaximumCount)
            {
                initialCount = MaximumCount;
            }

            InitialCount = initialCount;
        }

        public override object GetInitialValue() => null;

        public abstract Type ElementType { get; }

        protected abstract void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list);

        public virtual void PopulateList(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            list.Clear();
            for (int i = 0; i < InitialCount; i++)
            {
                AddDefaultValue(owner, propertyInfo, list);
            }
        }

        public void EnsureMinimumCount(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            while (list.Count < MinimumCount)
            {
                AddDefaultValue(owner, propertyInfo, list);
            }
        }

        public void AddNewItem(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            AddDefaultValue(owner, propertyInfo, list);

        public bool IsAtMaximumCount(IList list) => list != null && list.Count >= MaximumCount;

        public virtual bool IsCompatible(PropertyInfo propertyInfo)
        {
            if (!typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return false;
            }

            Type elementType = propertyInfo.GetListElementType();
            if (elementType == null)
            {
                return false;
            }

            return ElementType.IsAssignableFrom(elementType) || elementType.IsAssignableFrom(ElementType);
        }
    }
}

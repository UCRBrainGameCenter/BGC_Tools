using System;
using System.Collections;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ListFieldDisplayAttribute : FieldDisplayAttribute
    {
        public int MinimumCount { get; }
        public int MaximumCount { get; }

        protected ListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
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
        }

        public override object GetInitialValue() => null;

        public abstract Type ElementType { get; }

        protected virtual void AddInitialValues(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) { }

        protected abstract void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list);

        public virtual void PopulateList(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            list.Clear();
            AddInitialValues(owner, propertyInfo, list);
            ClampToMaximum(list);
            EnsureMinimumCount(owner, propertyInfo, list);
        }

        private void ClampToMaximum(IList list)
        {
            if (MaximumCount == int.MaxValue)
            {
                return;
            }

            while (list.Count > MaximumCount && list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public void EnsureMinimumCount(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            while (list.Count < MinimumCount)
            {
                if (IsAtMaximumCount(list))
                {
                    break;
                }

                AddDefaultValue(owner, propertyInfo, list);
            }
        }

        public void AddNewItem(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            if (IsAtMaximumCount(list))
            {
                return;
            }

            AddDefaultValue(owner, propertyInfo, list);
        }

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

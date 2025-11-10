using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PropertyGroupListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        private readonly Type[] selectionTypes;
        private readonly Type[] initialSelectionTypes;

        public IReadOnlyList<Type> SelectionTypes => selectionTypes;

        public PropertyGroupListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            Type[] selectionTypes,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            params Type[] initialSelectionTypes)
            : base(fieldName, displayTitle, minimumCount, maximumCount)
        {
            if (selectionTypes == null || selectionTypes.Length == 0)
            {
                throw new ArgumentException("At least one selection type must be provided.", nameof(selectionTypes));
            }

            this.selectionTypes = new Type[selectionTypes.Length];

            for (int i = 0; i < selectionTypes.Length; i++)
            {
                Type type = selectionTypes[i];
                if (!typeof(IPropertyGroup).IsAssignableFrom(type))
                {
                    throw new ArgumentException($"SelectionType must implement IPropertyGroup: {type}");
                }

                this.selectionTypes[i] = type;
            }

            if (initialSelectionTypes != null && initialSelectionTypes.Length > 0)
            {
                foreach (Type initialType in initialSelectionTypes)
                {
                    if (initialType == null)
                    {
                        throw new ArgumentException("Initial selection types cannot contain null entries.", nameof(initialSelectionTypes));
                    }

                    if (!selectionTypes.Contains(initialType))
                    {
                        throw new ArgumentException(
                            $"Initial selection type {initialType} must also be included in the selectionTypes list.",
                            nameof(initialSelectionTypes));
                    }
                }

                this.initialSelectionTypes = initialSelectionTypes.ToArray();
            }
            else
            {
                this.initialSelectionTypes = Array.Empty<Type>();
            }
        }

        public override Type ElementType => typeof(IPropertyGroup);

        public override bool IsCompatible(PropertyInfo propertyInfo)
        {
            if (!base.IsCompatible(propertyInfo))
            {
                return false;
            }

            Type elementType = propertyInfo.GetListElementType();
            if (elementType == null)
            {
                return false;
            }

            // Ensure the property can store every declared selection type.
            return selectionTypes.All(elementType.IsAssignableFrom);
        }

        protected override void AddDefaultValue(IPropertyGroup owner, PropertyInfo propertyInfo, IList list) =>
            list.Add(CreateEntry(owner, selectionTypes[0]));

        protected override void AddInitialValues(IPropertyGroup owner, PropertyInfo propertyInfo, IList list)
        {
            for (int i = 0; i < initialSelectionTypes.Length; i++)
            {
                if (IsAtMaximumCount(list))
                {
                    break;
                }

                list.Add(CreateEntry(owner, initialSelectionTypes[i]));
            }
        }

        public IPropertyGroup CreateEntry(IPropertyGroup owner, Type selectionType)
        {
            if (selectionType == null)
            {
                selectionType = selectionTypes[0];
            }

            if (!selectionTypes.Contains(selectionType))
            {
                throw new ArgumentException(
                    $"Type {selectionType} is not a valid selection for {fieldName}.",
                    nameof(selectionType));
            }

            IPropertyGroup entry = Activator.CreateInstance(selectionType) as IPropertyGroup;
            if (entry == null)
            {
                throw new ArgumentException($"Unable to construct property group of type {selectionType}.");
            }

            entry.SetParent(owner);
            entry.InitializeProperties();
            entry.BuildPartialPropertyGroup();

            return entry;
        }

        public Type GetSelectionType(string serializationName)
        {
            return selectionTypes.FirstOrDefault(type =>
                type.GetSelectionSerializationName() == serializationName);
        }
    }
}

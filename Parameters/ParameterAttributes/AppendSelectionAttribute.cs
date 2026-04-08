using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AppendSelectionAttribute : Attribute
    {
        public readonly Type[] selectionTypes;

        public AppendSelectionAttribute(params Type[] selectionTypes)
        {
            for (int i = 0; i < selectionTypes.Length; i++)
            {
                if (!typeof(IPropertyGroup).IsAssignableFrom(selectionTypes[i]))
                {
                    throw new ArgumentException($"SelectionType must implement IPropertyGroup: {selectionTypes[i]}");
                }
            }

            this.selectionTypes = selectionTypes.Clone() as Type[];
        }

        /// <summary>
        /// Called after a default instance is built via <see cref="IPropertyGroupExtensions.Build"/>.
        /// Override to apply custom default values to the newly constructed property group.
        /// </summary>
        public virtual void ApplyDefaults(IPropertyGroup builtGroup) { }
    }
    
}

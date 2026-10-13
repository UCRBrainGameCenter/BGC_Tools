using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AppendSelectionAttribute : Attribute
    {
        public readonly Type[] selectionTypes;

        /// <summary>
        /// Overrides the PropertyGroupTitle for this property in the UI.
        /// Falls back to PropertyGroupTitleAttribute if empty.
        /// </summary>
        public string DisplayTitle { get; set; } = "";

        /// <summary>Unit postfix inherited by non-unitless child fields.</summary>
        public string Postfix { get; set; } = "";

        /// <summary>Minimum constraint inherited by non-unitless child double fields.</summary>
        public double Minimum { get; set; } = double.MinValue;

        /// <summary>Maximum constraint inherited by non-unitless child double fields.</summary>
        public double Maximum { get; set; } = double.MaxValue;

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
        /// Called after a new instance of the property group is freshly built (not loaded).
        /// Override to apply context-sensitive initialization such as default values.
        /// </summary>
        public virtual void PostBuildInitialize(IPropertyGroup newInstance) { }
    }
    
}

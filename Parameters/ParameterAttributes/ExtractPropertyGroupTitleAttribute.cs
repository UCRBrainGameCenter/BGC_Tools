using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Attribute for Adaptive Algorithm Templates that instruct the UI system to use the internal
    /// Adaptive Property to generate the title of the property group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExtractPropertyGroupTitleAttribute : PropertyLabelAttribute
    {
        public ExtractPropertyGroupTitleAttribute() : base("", "") { }
    }
}

using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface | AttributeTargets.Class)]
    public class PropertyGroupTitleAttribute : PropertyLabelAttribute
    {
        public PropertyGroupTitleAttribute(string title, string serializationString = "")
            : base(title, serializationString)
        {
        }
    }
}

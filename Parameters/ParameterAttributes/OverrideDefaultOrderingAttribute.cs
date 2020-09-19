using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Used to order the properties that are shown for a IPropertyGroup
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OverrideDefaultOrderingAttribute : Attribute
    {
        public readonly int orderPriority;

        public OverrideDefaultOrderingAttribute(int orderPriority)
        {
            this.orderPriority = orderPriority;
        }
    }

}

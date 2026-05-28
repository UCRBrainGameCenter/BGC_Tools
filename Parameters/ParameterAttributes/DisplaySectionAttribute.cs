using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Declares a named section on an <see cref="IPropertyGroup"/>. Properties tagged with
    /// <see cref="InSectionAttribute"/> matching <see cref="Name"/> are grouped under a
    /// section header when the type is rendered in a column-stack FormColumn. Multiple
    /// attributes may be applied to one class to declare several sections; their relative
    /// order is taken from <see cref="Order"/> (lower renders first).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class DisplaySectionAttribute : Attribute
    {
        public readonly string Name;
        public readonly int Order;
        public readonly bool Collapsible;
        public readonly bool DefaultCollapsed;

        public DisplaySectionAttribute(string name, int order = 0, bool collapsible = false, bool defaultCollapsed = false)
        {
            Name = name;
            Order = order;
            Collapsible = collapsible;
            DefaultCollapsed = defaultCollapsed;
        }
    }
}

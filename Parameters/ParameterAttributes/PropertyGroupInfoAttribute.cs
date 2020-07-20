using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyGroupInfoAttribute : Attribute
    {
        public readonly string text;

        public PropertyGroupInfoAttribute(string text)
        {
            this.text = text;
        }
    }
}

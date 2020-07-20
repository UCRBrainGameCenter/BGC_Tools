using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyGroupItemTitleAttribute : Attribute
    {
        public readonly string serializationName;

        public PropertyGroupItemTitleAttribute(
            string serializationName = "")
        {
            this.serializationName = serializationName;
        }
    }
}

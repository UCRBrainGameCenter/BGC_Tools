using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayInputFieldAttribute : Attribute
    {
        public readonly string fieldName;

        public DisplayInputFieldAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}

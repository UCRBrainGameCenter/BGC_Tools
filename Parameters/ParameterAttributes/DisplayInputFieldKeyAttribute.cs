using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayInputFieldKeyAttribute : Attribute
    {
        public readonly string fieldName;

        public DisplayInputFieldKeyAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}

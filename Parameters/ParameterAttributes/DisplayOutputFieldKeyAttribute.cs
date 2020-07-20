using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayOutputFieldKeyAttribute : Attribute
    {
        public readonly string fieldName;

        public DisplayOutputFieldKeyAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}

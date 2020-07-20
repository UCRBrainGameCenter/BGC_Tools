using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OutputFieldAttribute : Attribute
    {
        public readonly string fieldName;

        public OutputFieldAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}

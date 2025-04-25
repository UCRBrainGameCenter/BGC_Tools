using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayPropertyGroupInlineAttribute : Attribute
    {
        public string FieldName { get; }
        public string Title { get; }

        public DisplayPropertyGroupInlineAttribute(string fieldName, string title = null)
        {
            FieldName = fieldName;
            Title = title;
        }
    }
}

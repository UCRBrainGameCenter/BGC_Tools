using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class FieldDisplayAttribute : Attribute
    {
        public readonly string fieldName;
        public readonly string displayTitle;

        public FieldDisplayAttribute(
            string fieldName,
            string displayTitle)
        {
            this.fieldName = fieldName;
            this.displayTitle = displayTitle;
        }

        public abstract object GetInitialValue();
    }
}

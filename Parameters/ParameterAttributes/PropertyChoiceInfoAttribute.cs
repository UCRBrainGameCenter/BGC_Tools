using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PropertyChoiceInfoAttribute : Attribute
    {
        public readonly string text;

        public PropertyChoiceInfoAttribute(string text)
        {
            this.text = text;
        }
    }
}

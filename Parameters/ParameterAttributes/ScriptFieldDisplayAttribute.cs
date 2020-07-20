using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ScriptFieldDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string initial;

        public ScriptFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initial = "")
            : base(fieldName, displayTitle)
        {
            this.initial = initial;
        }

        public override object GetInitialValue() => initial;
    }
}

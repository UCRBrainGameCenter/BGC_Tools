using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Indicates that the current parameter search should redirect into locally hosted IControlled
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ControlledExtractionAttribute : FieldDisplayAttribute
    {
        public readonly string extractionFieldName;

        public ControlledExtractionAttribute(
            string fieldName,
            string extractionFieldName,
            string displayTitle = "")
            : base(fieldName, displayTitle)
        {
            this.extractionFieldName = extractionFieldName;
        }

        public override object GetInitialValue() => null;
    }
}

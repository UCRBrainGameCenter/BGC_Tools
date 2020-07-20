using System;

namespace BGC.Parameters
{
    /// <summary>
    /// FieldDisplayAttribute to catch parameters defined higher in the hierarchy
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FieldMirrorDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string mirroredFieldName;

        public FieldMirrorDisplayAttribute(
            string fieldName,
            string mirroredFieldName,
            string displayTitle)
            : base(fieldName, displayTitle)
        {
            this.mirroredFieldName = mirroredFieldName;
        }

        public override object GetInitialValue() => null;
    }
}

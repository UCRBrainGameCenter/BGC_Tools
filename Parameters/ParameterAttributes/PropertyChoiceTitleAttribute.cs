using System;

namespace BGC.Parameters
{
    [Flags]
    public enum ChoiceRenderingModifier
    {
        Normal = 0,
        Controlled = 1 << 0
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyChoiceTitleAttribute : PropertyLabelAttribute
    {
        public readonly ChoiceRenderingModifier renderingModifier;

        public PropertyChoiceTitleAttribute(
            string title,
            string serializationString = "",
            ChoiceRenderingModifier renderingModifier = ChoiceRenderingModifier.Normal)
            : base(title, serializationString)
        {
            this.renderingModifier = renderingModifier;
        }
    }
}

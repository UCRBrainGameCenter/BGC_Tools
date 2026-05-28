using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Assigns the tagged property to a named section declared via
    /// <see cref="DisplaySectionAttribute"/> on the containing type. Used by the FormColumn
    /// reflector to group adjacent fields under section headers. Properties without this
    /// attribute fall into the default un-named section (no header).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InSectionAttribute : Attribute
    {
        public readonly string SectionName;

        public InSectionAttribute(string sectionName)
        {
            SectionName = sectionName;
        }
    }
}

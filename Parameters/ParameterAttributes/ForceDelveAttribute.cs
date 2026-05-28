using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Opt-out marker for the column-stack FormColumn's inline-subgroup rendering: when
    /// applied to a <see cref="DisplayPropertyGroupInlineAttribute"/> or
    /// <see cref="AppendSelectionAttribute"/> property (or the IPropertyGroup type itself),
    /// forces the field to render as a delve row even when the inner group is small enough
    /// to qualify for inline rendering. Use sparingly — the default heuristic is usually
    /// what you want.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface)]
    public class ForceDelveAttribute : Attribute
    {
    }
}

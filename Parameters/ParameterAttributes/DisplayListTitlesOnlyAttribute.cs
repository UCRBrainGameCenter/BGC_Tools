using System;

namespace BGC.Parameters
{
    /// <summary>
    /// On a property-group type selected through [AppendSelection]: the editor shows only the
    /// group's [PropertyGroupList] lists, each item by its title with its list controls, for
    /// groups whose items are edited elsewhere (e.g. a stimulus collection, whose stimuli have
    /// their own editor).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    public class DisplayListTitlesOnlyAttribute : Attribute
    {
    }
}

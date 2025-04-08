using System;

namespace BGC.Parameters
{
    /// <summary>
    /// Labels IList containers of IPropertyGroups
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyGroupListAttribute : Attribute
    {
        public readonly string fieldName;
        public readonly bool autoSerialize; 

        public PropertyGroupListAttribute(string fieldName, bool autoSerialize = true)
        {
            this.fieldName = fieldName;
            this.autoSerialize = autoSerialize;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyGroupListAttributeSimple : Attribute
    {
        public readonly string fieldName;
        public readonly bool autoSerialize; 

        public PropertyGroupListAttributeSimple(string fieldName, bool autoSerialize = true)
        {
            this.fieldName = fieldName;
            this.autoSerialize = autoSerialize;
        }
    }
}

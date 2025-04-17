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
    
    /// <summary>
    /// Labels IList containers of Primitives (int, double, string)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimitiveListAttribute : Attribute
    {
        public readonly string fieldName;
        public readonly bool autoSerialize; 

        public PrimitiveListAttribute(string fieldName, bool autoSerialize = true)
        {
            this.fieldName = fieldName;
            this.autoSerialize = autoSerialize;
        }
    }
}

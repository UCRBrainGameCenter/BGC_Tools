using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Parameters
{
    /// <summary>
    /// Used to list the various objects than can be instantiated and added to a PropertyGroupList
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AppendAdditionAttribute : Attribute
    {
        public readonly Type[] additionTypes;

        public AppendAdditionAttribute(params Type[] additionTypes)
        {
            for (int i = 0; i < additionTypes.Length; i++)
            {
                if (!typeof(IPropertyGroup).IsAssignableFrom(additionTypes[i]))
                {
                    throw new ArgumentException($"AdditionType must implement IPropertyGroup: {additionTypes[i]}");
                }
            }

            this.additionTypes = additionTypes.Clone() as Type[];
        }
    }
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AppendAdditionAttributeSimple : Attribute
    {
        public readonly Type additionType;

        public AppendAdditionAttributeSimple(Type additionType)
        {
            this.additionType = additionType;
        }
    }
}

using System;
using LightJson;
using UnityEngine;
using UnityEngine.Scripting;

namespace BGC.Parameters
{
    /// <summary>
    /// Specifies that an attribute should be saved to a property group's __STATE__ JSON object during serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializableStateAttribute : Attribute
    {
        public string FieldName { get; }
        public JsonValue DefaultValue { get; }

        /// <summary>
        /// Specifies this property or field as a serializable state variable that can be serialized by IPropertyGroup.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="defaultValue"></param>
        public SerializableStateAttribute(string fieldName, string defaultValue)
        {
            this.FieldName = fieldName;
            this.DefaultValue = JsonValue.Parse(defaultValue);
        }
    }
}
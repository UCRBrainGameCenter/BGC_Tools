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
        /// <summary>The key name of the property to use for serializing/deserializing in JSON.</summary>
        public string FieldName { get; }

        /// <summary>
        /// The default value to use if the field is missing from a serialized JSON object when deserializing.
        /// </summary>
        public JsonValue DefaultValue { get; }

        /// <summary>
        /// Specifies this property or field as a serializable state variable that can be serialized by IPropertyGroup.
        /// </summary>
        /// <param name="fieldName">The key name of the property to use for serializing/deserializing in JSON.</param>
        /// <param name="defaultDeserializationValue">
        /// The default value to use if the field is missing from a serialized JSON object when deserializing.
        /// </param>
        public SerializableStateAttribute(string fieldName, string defaultDeserializationValue)
        {
            this.FieldName = fieldName;
            this.DefaultValue = JsonValue.Parse(defaultDeserializationValue);
        }
    }
}
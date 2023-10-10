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
        public readonly string fieldName;
        public readonly JsonValue defaultValue;

        /// <summary>
        /// Specifies this property or field as a serializable state variable that can be serialized by IPropertyGroup.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="defaultValue"></param>
        public SerializableStateAttribute(string fieldName, string defaultValue)
        {
            this.fieldName = fieldName;
            this.defaultValue = JsonValue.Parse(defaultValue);
            // Type defaultValueType = defaultValue.GetType();
            // switch (defaultValueType.Name)
            // {
            //     case "Single":
            //         this.defaultValue =  Convert.ToSingle(defaultValue);
            //         break;
            //
            //     case "Double":
            //         this.defaultValue =  Convert.ToDouble(defaultValue);
            //         break;
            //
            //     case "Int32":
            //         this.defaultValue =  Convert.ToInt32(defaultValue);
            //         break;
            //
            //     case "Int64":
            //         this.defaultValue =  Convert.ToInt64(defaultValue);
            //         break;
            //
            //     case "String":
            //         this.defaultValue =  Convert.ToString(defaultValue);
            //         break;
            //
            //     case "Boolean":
            //         this.defaultValue =  Convert.ToBoolean(defaultValue);
            //         break;
            //
            //     default:
            //         if (defaultValueType.IsEnum)
            //         {
            //             this.defaultValue = defaultValue.ToString();
            //             break;
            //         }
            //         Debug.LogError($"SerializableStateAttribute: Unsupported datatype ({defaultValueType.Name}) for variable {defaultValue}");
            //         break;
            // }
        }
    }
}
using System;
using System.Diagnostics;
using System.Collections.Generic;
using LightJson.Serialization;

namespace LightJson
{
    /// <summary>
    /// A wrapper object that contains a valid JSON value.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}", Type = "JsonValue({Type})")]
    [DebuggerTypeProxy(typeof(JsonValueDebugView))]
    public readonly struct JsonValue
    {
        private readonly JsonValueType type;
        private readonly object reference;
        private readonly double value;

        /// <summary>Represents a null JsonValue.</summary>
        public static readonly JsonValue Null = new JsonValue(JsonValueType.Null, default(double), null);

        /// <summary>The type of this JsonValue.</summary>
        public JsonValueType Type => type;

        /// <summary>Indicates whether this JsonValue is Null.</summary>
        public bool IsNull => Type == JsonValueType.Null;

        /// <summary>Indicates whether this JsonValue is a Boolean.</summary>
        public bool IsBoolean => Type == JsonValueType.Boolean;

        /// <summary>Indicates whether this JsonValue is an Integer.</summary>
        public bool IsInteger
        {
            get
            {
                if (!IsNumber)
                {
                    return false;
                }

                //Why is this extra copy necessary?
                double value = this.value;

                return (value >= int.MinValue) && (value <= int.MaxValue) && unchecked((int)value) == value;
            }
        }

        /// <summary>Indicates whether this JsonValue is a Number.</summary>
        public bool IsNumber => Type == JsonValueType.Number;

        /// <summary>Indicates whether this JsonValue is a String.</summary>
        public bool IsString => Type == JsonValueType.String;

        /// <summary>Indicates whether this JsonValue is a JsonObject.</summary>
        public bool IsJsonObject => Type == JsonValueType.Object;

        /// <summary>Indicates whether this JsonValue is a JsonArray.</summary>
        public bool IsJsonArray => Type == JsonValueType.Array;

        /// <summary>Indicates whether this JsonValue represents a DateTime.</summary>
        public bool IsDateTime => AsDateTime != null;

        /// <summary>This value as a Boolean type.</summary>
        public bool AsBoolean
        {
            get
            {
                switch (Type)
                {
                    case JsonValueType.Boolean: return (value == 1);
                    case JsonValueType.Number: return (value != 0);
                    case JsonValueType.String: return ((string)reference != "");
                    case JsonValueType.Object:
                    case JsonValueType.Array: return true;
                    default: return false;
                }
            }
        }

        /// <summary>This value as an Integer type.</summary>
        public int AsInteger
        {
            get
            {
                double value = AsNumber;

                // Prevent overflow if the value doesn't fit.
                if (value >= int.MaxValue)
                {
                    return int.MaxValue;
                }

                if (value <= int.MinValue)
                {
                    return int.MinValue;
                }

                return (int)value;
            }
        }

        /// <summary>This value as a Number type.</summary>
        public double AsNumber
        {
            get
            {
                switch (Type)
                {
                    case JsonValueType.Boolean:
                        return (value == 1) ? 1 : 0;

                    case JsonValueType.Number:
                        return value;

                    case JsonValueType.String:
                        double number;
                        if (double.TryParse((string)reference, out number))
                        {
                            return number;
                        }
                        goto default;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>This value as a String type.</summary>
        public string AsString
        {
            get
            {
                switch (Type)
                {
                    case JsonValueType.Boolean:
                        return (value == 1) ? "true" : "false";

                    case JsonValueType.Number:
                        return value.ToString();

                    case JsonValueType.String:
                        return (string)reference;

                    default:
                        return null;
                }
            }
        }

        /// <summary>This value as an JsonObject.</summary>
        public JsonObject AsJsonObject => IsJsonObject ? (JsonObject)reference : null;

        /// <summary>This value as an JsonArray.</summary>
        public JsonArray AsJsonArray => IsJsonArray ? (JsonArray)reference : null;

        /// <summary>This value as a System.DateTime.</summary>
        public DateTime? AsDateTime
        {
            get
            {
                DateTime value;

                if (IsString && DateTime.TryParse((string)reference, out value))
                {
                    return value;
                }

                return null;
            }
        }

        /// <summary>This (inner) value as a System.object.</summary>
        public object AsObject
        {
            get
            {
                switch (Type)
                {
                    case JsonValueType.Boolean:
                    case JsonValueType.Number:
                        return value;

                    case JsonValueType.String:
                    case JsonValueType.Object:
                    case JsonValueType.Array:
                        return reference;

                    default:
                        return null;
                }
            }
        }

        /// <summary>The value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this JsonValue is not a JsonObject.
        /// </exception>
        public JsonValue this[string key]
        {
            get
            {
                if (IsJsonObject == false)
                {
                    throw new InvalidOperationException("This value does not represent a JsonObject.");
                }

                return ((JsonObject)reference)[key];
            }
            set
            {
                if (IsJsonObject == false)
                {
                    throw new InvalidOperationException("This value does not represent a JsonObject.");
                }

                ((JsonObject)reference)[key] = value;
            }
        }

        /// <summary>The value at the specified index.</summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this JsonValue is not a JsonArray
        /// </exception>
        public JsonValue this[int index]
        {
            get
            {
                if (IsJsonArray == false)
                {
                    throw new InvalidOperationException("This value does not represent a JsonArray.");
                }

                return ((JsonArray)reference)[index];
            }
            set
            {
                if (IsJsonArray == false)
                {
                    throw new InvalidOperationException("This value does not represent a JsonArray.");
                }

                ((JsonArray)reference)[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct.
        /// </summary>
        /// <param name="type">The Json type of the JsonValue.</param>
        /// <param name="value">
        /// The internal value of the JsonValue.
        /// This is used when the Json type is Number or Boolean.
        /// </param>
        /// <param name="reference">
        /// The internal value reference of the JsonValue.
        /// This value is used when the Json type is String, JsonObject, or JsonArray.
        /// </param>
        private JsonValue(JsonValueType type, double value, object reference)
        {
            this.type = type;
            this.value = value;
            this.reference = reference;
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct, representing a Boolean value.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        public JsonValue(bool? value)
        {
            if (value.HasValue)
            {
                type = JsonValueType.Boolean;
                this.value = value.Value ? 1 : 0;
                reference = null;
            }
            else
            {
                this = Null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct, representing a Number value.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        public JsonValue(double? value)
        {
            if (value.HasValue)
            {
                type = JsonValueType.Number;
                this.value = value.Value;
                reference = null;
            }
            else
            {
                this = Null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct, representing a String value.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        public JsonValue(string value)
        {
            if (value != null)
            {
                type = JsonValueType.String;
                this.value = default(double);
                reference = value;
            }
            else
            {
                this = Null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct, representing a JsonObject.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        public JsonValue(JsonObject value)
        {
            if (value != null)
            {
                type = JsonValueType.Object;
                this.value = default(double);
                reference = value;
            }
            else
            {
                this = Null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the JsonValue struct, representing a Array reference value.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        public JsonValue(JsonArray value)
        {
            if (value != null)
            {
                type = JsonValueType.Array;
                this.value = default(double);
                reference = value;
            }
            else
            {
                this = Null;
            }
        }

        /// <summary>
        /// Converts the given nullable boolean into a JsonValue.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(bool? value) => new JsonValue(value);

        /// <summary>
        /// Converts the given nullable double into a JsonValue.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(double? value) => new JsonValue(value);

        /// <summary>
        /// Converts the given string into a JsonValue.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(string value) => new JsonValue(value);

        /// <summary>
        /// Converts the given JsonObject into a JsonValue.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(JsonObject value) => new JsonValue(value);

        /// <summary>
        /// Converts the given JsonArray into a JsonValue.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(JsonArray value) => new JsonValue(value);

        /// <summary>
        /// Converts the given DateTime? into a JsonValue.
        /// </summary>
        /// <remarks>
        /// The DateTime value will be stored as a string using ISO 8601 format,
        /// since JSON does not define a DateTime type.
        /// </remarks>
        /// <param name="value">The value to be converted.</param>
        public static implicit operator JsonValue(DateTime? value) =>
            value.HasValue ? new JsonValue(value.Value.ToString("o")) : Null;


        /// <summary>
        /// Converts the given JsonValue into an Int.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator int(JsonValue jsonValue) =>
            jsonValue.IsInteger ? jsonValue.AsInteger : 0;

        /// <summary>
        /// Converts the given JsonValue into a nullable Int.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        /// <exception cref="System.InvalidCastException">
        /// Throws System.InvalidCastException when the inner value type of the
        /// JsonValue is not the desired type of the conversion.
        /// </exception>
        public static implicit operator int? (JsonValue jsonValue) =>
            jsonValue.IsNull ? (int?)null : (int)jsonValue;


        /// <summary>
        /// Converts the given JsonValue into a Bool.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator bool(JsonValue jsonValue) =>
            jsonValue.IsBoolean ? (jsonValue.value == 1) : false;


        /// <summary>
        /// Converts the given JsonValue into a nullable Bool.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        /// <exception cref="System.InvalidCastException">
        /// Throws System.InvalidCastException when the inner value type of the
        /// JsonValue is not the desired type of the conversion.
        /// </exception>
        public static implicit operator bool? (JsonValue jsonValue) =>
            jsonValue.IsNull ? (bool?)null : (bool)jsonValue;


        /// <summary>
        /// Converts the given JsonValue into a Double.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator double(JsonValue jsonValue) =>
            jsonValue.IsNumber ? jsonValue.value : double.NaN;

        /// <summary>
        /// Converts the given JsonValue into a nullable Double.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        /// <exception cref="System.InvalidCastException">
        /// Throws System.InvalidCastException when the inner value type of the
        /// JsonValue is not the desired type of the conversion.
        /// </exception>
        public static implicit operator double? (JsonValue jsonValue) =>
            jsonValue.IsNull ? (double?)null : (double)jsonValue;

        /// <summary>
        /// Converts the given JsonValue into a String.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator string(JsonValue jsonValue) =>
            (jsonValue.IsString || jsonValue.IsNull) ? jsonValue.reference as string : null;

        /// <summary>
        /// Converts the given JsonValue into a JsonObject.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator JsonObject(JsonValue jsonValue) =>
            (jsonValue.IsJsonObject || jsonValue.IsNull) ? jsonValue.reference as JsonObject : null;

        /// <summary>
        /// Converts the given JsonValue into a JsonArray.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator JsonArray(JsonValue jsonValue) =>
            (jsonValue.IsJsonArray || jsonValue.IsNull) ? jsonValue.reference as JsonArray : null;

        /// <summary>
        /// Converts the given JsonValue into a DateTime.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator DateTime(JsonValue jsonValue)
        {
            DateTime? dateTime = jsonValue.AsDateTime;

            if (dateTime.HasValue)
            {
                return dateTime.Value;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Converts the given JsonValue into a nullable DateTime.
        /// </summary>
        /// <param name="jsonValue">The JsonValue to be converted.</param>
        public static implicit operator DateTime? (JsonValue jsonValue) =>
            (jsonValue.IsDateTime || jsonValue.IsNull) ? jsonValue.AsDateTime : null;


        /// <summary>
        /// Returns a value indicating whether the two given JsonValues are equal.
        /// </summary>
        /// <param name="a">A JsonValue to compare.</param>
        /// <param name="b">A JsonValue to compare.</param>
        public static bool operator ==(JsonValue a, JsonValue b)
        {
            return a.Type == b.Type && 
                a.value == b.value &&
                Equals(a.reference, b.reference);
        }

        /// <summary>
        /// Returns a value indicating whether the two given JsonValues are unequal.
        /// </summary>
        /// <param name="a">A JsonValue to compare.</param>
        /// <param name="b">A JsonValue to compare.</param>
        public static bool operator !=(JsonValue a, JsonValue b) => !(a == b);

        /// <summary>
        /// Returns a JsonValue by parsing the given string.
        /// </summary>
        /// <param name="text">The JSON-formatted string to be parsed.</param>
        public static JsonValue Parse(string text) => JsonReader.Parse(text);

        /// <summary>
        /// Returns a value indicating whether this JsonValue is equal to the given object.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                //They are considered equal if both objects are null
                return IsNull;
            }

            JsonValue? jsonValue = obj as JsonValue?;

            if (jsonValue.HasValue)
            {
                //Check the actual values with the overloaded operator
                return (this == jsonValue.Value);
            }
            
            //Types don't allow for conversion to a nullable-JsonValue
            return false;
        }

        /// <summary>
        /// Returns a hash code for this JsonValue.
        /// </summary>
        public override int GetHashCode()
        {
            if (IsNull)
            {
                return Type.GetHashCode();
            }

            return Type.GetHashCode() ^
                value.GetHashCode() ^
                EqualityComparer<object>.Default.GetHashCode(reference);
        }

        /// <summary>
        /// Returns a JSON string representing the state of the object.
        /// </summary>
        /// <remarks>
        /// The resulting string is safe to be inserted as is into dynamically
        /// generated JavaScript or JSON code.
        /// </remarks>
        public override string ToString() => ToString(false);

        /// <summary>
        /// Returns a JSON string representing the state of the object.
        /// </summary>
        /// <remarks>
        /// The resulting string is safe to be inserted as is into dynamically
        /// generated JavaScript or JSON code.
        /// </remarks>
        /// <param name="pretty">
        /// Indicates whether the resulting string should be formatted for human-readability.
        /// </param>
        public string ToString(bool pretty) => JsonWriter.Serialize(this, pretty);

        private class JsonValueDebugView
        {
            private JsonValue jsonValue;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public JsonObject ObjectView =>
                jsonValue.IsJsonObject ? (JsonObject)jsonValue.reference : null;


            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public JsonArray ArrayView =>
                jsonValue.IsJsonArray ? (JsonArray)jsonValue.reference : null;


            public JsonValueType Type => jsonValue.Type;

            public object Value
            {
                get
                {
                    if (jsonValue.IsJsonObject)
                    {
                        return (JsonObject)jsonValue.reference;
                    }
                    else if (jsonValue.IsJsonArray)
                    {
                        return (JsonArray)jsonValue.reference;
                    }

                    return jsonValue;
                }
            }

            public JsonValueDebugView(JsonValue jsonValue)
            {
                this.jsonValue = jsonValue;
            }
        }
    }
}

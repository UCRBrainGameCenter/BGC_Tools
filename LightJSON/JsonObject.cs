using System;
using System.Collections.Generic;
using System.Diagnostics;
using LightJson.Serialization;

namespace LightJson
{
    /// <summary>
    /// Represents a key-value pair collection of JsonValue objects.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(JsonObjectDebugView))]
    public sealed class JsonObject : IEnumerable<KeyValuePair<string, JsonValue>>, IEnumerable<JsonValue>
    {
        private Dictionary<string, JsonValue> properties = new Dictionary<string, JsonValue>();

        /// <summary>The number of properties in this JsonObject.</summary>
        public int Count => properties.Count;

        /// <summary>The property with the given key.</summary>
        /// <param name="key">The key of the property to get or set.</param>
        /// <remarks> Returns JsonValue.Null if the given key is not assosiated with any value. </remarks>
        public JsonValue this[string key]
        {
            get
            {
                JsonValue value;

                if (properties.TryGetValue(key, out value))
                {
                    return value;
                }

                return JsonValue.Null;
            }
            set
            {
                properties[key] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of JsonObject.
        /// </summary>
        public JsonObject() { }

        /// <summary>
        /// Adds a key with a null value to this collection.
        /// </summary>
        /// <param name="key">The key of the property to be added.</param>
        /// <remarks>Returns this JsonObject.</remarks>
        public JsonObject Add(string key) => Add(key, JsonValue.Null);

        /// <summary>
        /// Adds a value associated with a key to this collection.
        /// </summary>
        /// <param name="key">The key of the property to be added.</param>
        /// <param name="value">The value of the property to be added.</param>
        /// <returns>Returns this JsonObject.</returns>
        public JsonObject Add(string key, JsonValue value)
        {
            properties.Add(key, value);
            return this;
        }

        /// <summary>
        /// Adds a value associated with a key to this collection only if the value is not null.
        /// </summary>
        /// <param name="key">The key of the property to be added.</param>
        /// <param name="value">The value of the property to be added.</param>
        /// <returns>Returns this JsonObject.</returns>
        public JsonObject AddIfNotNull(string key, JsonValue value)
        {
            if (value.IsNull == false)
            {
                Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Removes a property with the given key.
        /// </summary>
        /// <param name="key">The key of the property to be removed.</param>
        /// <returns>
        /// Returns true if the given key is found and removed; otherwise, false.
        /// </returns>
        public bool Remove(string key) => properties.Remove(key);

        /// <summary>
        /// Clears the contents of this collection.
        /// </summary>
        /// <returns>Returns this JsonObject.</returns>
        public JsonObject Clear()
        {
            properties.Clear();
            return this;
        }

        /// <summary>
        /// Changes the key of one of the items in the collection.
        /// </summary>
        /// <remarks>
        /// This method has no effects if the <i>oldKey</i> does not exists.
        /// If the <i>newKey</i> already exists, the value will be overwritten.
        /// </remarks>
        /// <param name="oldKey">The name of the key to be changed.</param>
        /// <param name="newKey">The new name of the key.</param>
        /// <returns>Returns this JsonObject.</returns>
        public JsonObject Rename(string oldKey, string newKey)
        {
            JsonValue value;

            if (properties.TryGetValue(oldKey, out value))
            {
                Remove(oldKey);
                this[newKey] = value;
            }

            return this;
        }

        /// <summary>
        /// Determines whether this collection contains an item assosiated with the given key.
        /// </summary>
        /// <param name="key">The key to locate in this collection.</param>
        /// <returns>Returns true if the key is found; otherwise, false.</returns>
        public bool ContainsKey(string key) => properties.ContainsKey(key);

        /// <summary>
        /// Determines whether this collection contains an item assosiated with the given key.
        /// </summary>
        /// <param name="key">The key to locate in this collection.</param>
        /// <param name="value">
        /// When this method returns, this value gets assigned the JsonValue assosiated with
        /// the key, if the key is found; otherwise, JsonValue.Null is assigned.
        /// </param>
        /// <returns>Returns true if the key is found; otherwise, false.</returns>
        public bool ContainsKey(string key, out JsonValue value) => properties.TryGetValue(key, out value);

        /// <summary>
        /// Returns the value associated with a key, or a default value if not found.
        /// </summary>
        /// <param name="key">The key to locate in this collection.</param>
        /// <param name="defaultValue">The default JsonValue to use if the key is not found</param>
        /// <returns>Returns the JsonValue that was found, or defaultValue if no key was found.</returns>
        public JsonValue Get(string key, JsonValue defaultValue)
        {
            JsonValue value;

            if (properties.TryGetValue(key, out value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Determines whether this collection contains the given JsonValue.
        /// </summary>
        /// <param name="value">The value to locate in this collection.</param>
        /// <returns>Returns true if the value is found; otherwise, false.</returns>
        public bool Contains(JsonValue value) => properties.ContainsValue(value);

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator() => properties.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => properties.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

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

        private class JsonObjectDebugView
        {
            private JsonObject jsonObject;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair[] Keys
            {
                get
                {
                    KeyValuePair[] keys = new KeyValuePair[jsonObject.Count];

                    int i = 0;
                    foreach (KeyValuePair<string, JsonValue> property in jsonObject)
                    {
                        keys[i] = new KeyValuePair(jsonObject, property.Key, property.Value);
                        i += 1;
                    }

                    return keys;
                }
            }

            public JsonObjectDebugView(JsonObject jsonObject)
            {
                this.jsonObject = jsonObject;
            }

            [DebuggerDisplay("{value.ToString(),nq}", Name = "{key,nq}", Type = "JsonValue({Type})")]
            public class KeyValuePair
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string key;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private JsonValue value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private JsonValueType Type => value.Type;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private JsonObject parent;

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public object View
                {
                    get
                    {
                        if (value.IsJsonObject)
                        {
                            return (JsonObject)value;
                        }
                        else if (value.IsJsonArray)
                        {
                            return (JsonArray)value;
                        }

                        return value;
                    }
                }

                public string Key
                {
                    get { return key; }
                    set
                    {
                        JsonValue tempValue = parent[key];
                        parent.Remove(key);
                        key = value;
                        parent.Add(key, tempValue);
                    }
                }

                public KeyValuePair(JsonObject parent, string key, JsonValue value)
                {
                    this.parent = parent;
                    this.key = key;
                    this.value = value;
                }
            }
        }
    }
}

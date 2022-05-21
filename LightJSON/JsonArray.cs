using System;
using System.Collections.Generic;
using System.Diagnostics;
using LightJson.Serialization;

namespace LightJson
{
    /// <summary> Represents an ordered collection of JsonValues. </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(JsonArrayDebugView))]
    public sealed class JsonArray : IEnumerable<JsonValue>
    {
        private List<JsonValue> items = new List<JsonValue>();

        /// <summary>The number of values in this collection.</summary>
        public int Count => items.Count;

        /// <summary>The value at the given index.</summary>
        /// <param name="index">The zero-based index of the value.</param>
        /// <remarks> Will return JsonValue.Null if the given index is out of range. </remarks>
        public JsonValue this[int index]
        {
            get
            {
                if (index >= 0 && index < items.Count)
                {
                    return items[index];
                }

                return JsonValue.Null;
            }
            set
            {
                items[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of JsonArray.
        /// </summary>
        public JsonArray() { }

        /// <summary>
        /// Initializes a new instance and adds the given values to the collection.
        /// </summary>
        /// <param name="values">The values to be added to this collection.</param>
        public JsonArray(IEnumerable<JsonValue> values) : this()
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            foreach (JsonValue value in values)
            {
                items.Add(value);
            }
        }

        /// <summary>
        /// Initializes a new instance and adds the given values to the collection.
        /// </summary>
        /// <param name="values">The values to be added to this collection.</param>
        public JsonArray(params JsonValue[] values) : this()
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            foreach (JsonValue value in values)
            {
                items.Add(value);
            }
        }

        /// <summary>
        /// Adds the given value to this collection.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        /// <returns>Returns this collection.</returns>
        public JsonArray Add(JsonValue value)
        {
            items.Add(value);
            return this;
        }

        /// <summary>
        /// Adds the given value to this collection only if the value is not null.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        /// <returns>Returns this collection.</returns>
        public JsonArray AddIfNotNull(JsonValue value)
        {
            if (!value.IsNull)
            {
                Add(value);
            }

            return this;
        }

        /// <summary>
        /// Inserts the given value at the given index in this collection.
        /// </summary>
        /// <param name="index">The index where the given value will be inserted.</param>
        /// <param name="value">The value to be inserted into this collection.</param>
        /// <returns>Returns this collection.</returns>
        public JsonArray Insert(int index, JsonValue value)
        {
            items.Insert(index, value);
            return this;
        }

        /// <summary>
        /// Inserts the given value at the given index in this collection if it is not null.
        /// </summary>
        /// <param name="index">The index where the given value will be inserted.</param>
        /// <param name="value">The value to be inserted into this collection.</param>
        /// <returns>Returns this collection.</returns>
        public JsonArray InsertIfNotNull(int index, JsonValue value)
        {
            if (!value.IsNull)
            {
                Insert(index, value);
            }

            return this;
        }

        /// <summary>
        /// Removes the value at the given index.
        /// </summary>
        /// <param name="index">The index of the value to be removed.</param>
        /// <returns>Return this collection.</returns>
        public JsonArray Remove(int index)
        {
            items.RemoveAt(index);
            return this;
        }

        /// <summary>
        /// Clears the contents of this collection.
        /// </summary>
        /// <returns>Returns this collection.</returns>
        public JsonArray Clear()
        {
            items.Clear();
            return this;
        }

        /// <summary>
        /// Determines whether the given item is in the JsonArray.
        /// </summary>
        /// <param name="item">The item to locate in the JsonArray.</param>
        /// <returns>Returns true if the item is found; otherwise, false.</returns>
        public bool Contains(JsonValue item) => items.Contains(item);

        /// <summary>
        /// Determines the index of the given item in this JsonArray.
        /// </summary>
        /// <param name="item">The item to locate in this JsonArray.</param>
        /// <returns>The index of the item, if found. Otherwise, returns -1.</returns>
        public int IndexOf(JsonValue item) => items.IndexOf(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<JsonValue> GetEnumerator() => items.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns a JSON string representing the state of the array.
        /// </summary>
        /// <remarks>
        /// The resulting string is safe to be inserted as is into dynamically
        /// generated JavaScript or JSON code.
        /// </remarks>
        public override string ToString() => ToString(false);

        /// <summary>
        /// Returns a JSON string representing the state of the array.
        /// </summary>
        /// <remarks>
        /// The resulting string is safe to be inserted as is into dynamically
        /// generated JavaScript or JSON code.
        /// </remarks>
        /// <param name="pretty">
        /// Indicates whether the resulting string should be formatted for human-readability.
        /// </param>
        public string ToString(bool pretty) => JsonWriter.Serialize(this, pretty);

        private class JsonArrayDebugView
        {
            private JsonArray jsonArray;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public JsonValue[] Items
            {
                get
                {
                    JsonValue[] items = new JsonValue[jsonArray.Count];

                    for (int i = 0; i < jsonArray.Count; i += 1)
                    {
                        items[i] = jsonArray[i];
                    }

                    return items;
                }
            }

            public JsonArrayDebugView(JsonArray jsonArray)
            {
                this.jsonArray = jsonArray;
            }
        }
    }
}

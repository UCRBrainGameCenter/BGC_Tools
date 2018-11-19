using System.Collections.Generic;
using BGC.Extensions;
using UnityEngine;
using LightJson;
using System;

namespace BGC.DataStructures
{
    /// <summary>
    /// A Serializeable List<int> for lists of lists
    /// </summary>
    [Serializable]
    public class IntListContainer
    {
        [SerializeField]
        public List<int> list = new List<int>();

        /// <summary>
        /// Get value at index i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int this[int i]
        {
            get
            {
                return list[i];
            }
            set
            {
                list[i] = value;
            }
        }

        /// <summary>
        /// Get json array representation of the list
        /// </summary>
        public JsonArray JsonArray => list.IntListToJsonArray();

        /// <summary>
        /// Get a clean clone of this as a new int list container
        /// </summary>
        public IntListContainer Clone => new IntListContainer(new List<int>(list));

        /// <summary>
        /// Get a random value in the list
        /// </summary>
        public int RandomValue => list.RandomValue<int>();

        /// <summary>
        /// Get the number of elements in the list
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Constructor from a List
        /// </summary>
        /// <param name="list"></param>
        public IntListContainer(List<int> list)
        {
            if (list == null)
            {
                this.list = new List<int>();
            }
            else
            {
                this.list = new List<int>(list);
            }
        }

        /// <summary>
        /// Constructor from paramater list
        /// </summary>
        /// <param name="list"></param>
        public IntListContainer(params int[] list)
        {
            if (list == null)
            {
                this.list = new List<int>();
            }
            else
            {
                this.list = new List<int>(list);
            }
        }

        /// <summary>
        /// Constructor from a JsonArray
        /// </summary>
        /// <param name="json"></param>
        public IntListContainer(JsonArray json)
        {
            if (json == null)
            {
                list = new List<int>();
            }
            else
            {
                Deserialize(json);
            }
        }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public IntListContainer()
        {
            list = new List<int>();
        }

        /// <summary>
        /// Adds an element to a list
        /// </summary>
        /// <param name="element"></param>
        public void Add(int element)
        {
            list.Add(element);
        }

        /// <summary>
        /// Removes an element at specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        /// <summary>
        /// Remove an element from a list
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Remove(int element)
        {
            return list.Remove(element);
        }

        /// <summary>
        /// Remove all elements in another IntListContainer
        /// Returns false if failed to remove one element
        /// </summary>
        /// <param name="lw"></param>
        /// <returns></returns>
        public bool Remove(IntListContainer lw)
        {
            bool removed = true;

            for (int i = 0; i < lw.Count; ++i)
            {
                removed = Remove(lw[i]) && removed;
            }

            return removed;
        }

        /// <summary>
        /// Check if two IntListContainers are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                if(obj.GetType() == typeof(List<int>))
                {
                    return ListExtension.ListsEquivalent(list, (List<int>)obj);
                }

                return false;
            }

            return ListExtension.ListsEquivalent(list, ((IntListContainer)obj).list);
        }

        /// <summary>
        /// Returns HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return list.GetSequenceHashCode();
        }

        /// <summary>
        /// Debug.Log to print out all values of the list
        /// </summary>
        public void PrintSelf()
        {
            list.PrintSelf();
        }

        /// <summary>
        /// Check if the list Contains an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(int item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Sets the list to a deserialized JsonArray
        /// </summary>
        /// <param name="array"></param>
        protected void Deserialize(JsonArray array)
        {
            list = new List<int>();
            int count = array.Count;
            JsonValue arrayValue;

            for (int i = 0; i < count; ++i)
            {
                arrayValue = array[i];
                if (arrayValue.IsInteger)
                {
                    list.Add(arrayValue.AsInteger);
                }
                else
                {
                    throw new ArgumentException("Json array must be composed of only integers");
                }
            }
        }
    }
}
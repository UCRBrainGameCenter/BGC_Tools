using System.Collections.Generic;
using BGC.Extensions;
using UnityEngine;
using LightJson;

namespace BGC.DataStructures
{
    /// <summary>
    /// A Serializeable List<int> for 2D Lists
    /// </summary>
    [System.Serializable]
    public class IntListContainer
    {
        [SerializeField]
        public List<int> list = new List<int>();

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

        public IntListContainer Clone => new IntListContainer(new List<int>(list));
        public int RandomValue => list.RandomValue<int>();
        public int Count => list.Count;

        /// <summary>
        /// Constructor from a List
        /// </summary>
        /// <param name="list"></param>
        public IntListContainer(List<int> list)
        {
            this.list = new List<int>(list);
        }

        /// <summary>
        /// Constructor from a JsonArray
        /// </summary>
        /// <param name="json"></param>
        public IntListContainer(JsonArray json)
        {
            Deserialize(json);
        }

        /// <summary>
        /// Default constructor
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
        /// Set contained list
        /// </summary>
        /// <param name="list"></param>
        public void Set(List<int> list)
        {
            this.list = list;
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

            if (obj.GetType() != this.GetType())
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
        /// Serializes the list into a json array
        /// </summary>
        /// <returns></returns>
        public JsonArray Serialize()
        {
            return list.IntListToJsonArray();
        }

        /// <summary>
        /// Sets the list to a deserialized JsonArray
        /// </summary>
        /// <param name="array"></param>
        public void Deserialize(JsonArray array)
        {
            this.list = array.JsonArrayToIntList();
        }
    }
}
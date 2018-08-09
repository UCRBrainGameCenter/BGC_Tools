using System.Collections.Generic;
using BGC.Extensions;
using LightJson;
using System;
using BGC.Utility;

namespace BGC.DataStructures
{
    // @todo: When updated to C# 7.3 Remove all System type Checks and add where TEnum : Enum
    /// <summary>
    /// A Serializeable List<EnumType> for 2D lists
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    [Serializable]
    public class EnumListContainer<TEnum> : IntListContainer
    {
        public new TEnum this[int i]
        {
            get
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), list[i]);
            }
            set
            {
                list[i] = (int)Convert.ChangeType(value, typeof(int));
            }
        }

        public new EnumListContainer<TEnum> Clone => new EnumListContainer<TEnum>(new List<int>(list));
        public new TEnum RandomValue => (TEnum)Convert.ChangeType(list.RandomValue<int>(), typeof(TEnum));

        /// <summary>
        /// Constructor from raw int enum values
        /// </summary>
        /// <param name="list"></param>
        public EnumListContainer(List<int> list) : base(list) { }

        /// <summary>
        /// Constructor from List of enum values
        /// </summary>
        /// <param name="list"></param>
        public EnumListContainer(List<TEnum> list)
        {
            CheckIfEnumType();

            List<int> temp = new List<int>();
            for(int i = 0; i < list.Count; ++i)
            {
                temp.Add((int)Convert.ChangeType(list[i], typeof(int)));
            }

            this.list = temp;
        }

        /// <summary>
        /// Constructor from a JsonArray of Ints
        /// </summary>
        /// <param name="json"></param>
        public EnumListContainer(JsonArray json)
        {
            Deserialize(json);
        }

        /// <summary>
        /// Constructor from a JsonArray of EnumStrings
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serialization"></param>
        public EnumListContainer(JsonArray json, EnumSerialization serialization)
        {
            Deserialize(json, serialization);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EnumListContainer()
        {
            list = new List<int>();
        }

        /// <summary>
        /// Adds an element of type TEnum to a list
        /// </summary>
        /// <param name="element"></param>
        public void Add(TEnum element)
        {
            list.Add((int)Convert.ChangeType(element, typeof(int)));
        }

        /// <summary>
        /// Removes an element of type TEnum from a list
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Remove(TEnum element)
        {
            return list.Remove((int)Convert.ChangeType(element, typeof(int)));
        }

        /// <summary>
        /// Remove all elements in another EnumListContainer
        /// Returns false if failed to remove one element
        /// </summary>
        /// <param name="elc"></param>
        /// <returns></returns>
        public bool Remove(EnumListContainer<TEnum> elc)
        {
            bool removed = true;

            for (int i = 0; i < elc.Count; ++i)
            {
                removed = Remove(elc[i]) && removed;
            }

            return removed;
        }

        /// <summary>
        /// Set contained list
        /// </summary>
        /// <param name="list"></param>
        public void Set(List<TEnum> list)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < list.Count; ++i)
            {
                temp.Add((int)Convert.ChangeType(list[i], typeof(int)));
            }

            this.list = temp;
        }

        /// <summary>
        /// Check if two EnumListContainers are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return ListExtension.ListsEquivalent(list, ((EnumListContainer<TEnum>)obj).list);
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
        /// Check if the list contains an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(TEnum item)
        {
            return list.Contains((int)Convert.ChangeType(item, typeof(int)));
        }

        /// <summary>
        /// Serializes the list into a json array
        /// </summary>
        /// <returns></returns>
        public new JsonArray Serialize()
        {
            List<TEnum> enumList = new List<TEnum>();
            for(int i = 0; i < list.Count; ++i)
            {
                enumList.Add((TEnum)Convert.ChangeType(list[i], typeof(TEnum)));
            }

            return enumList.AnyListToStringJsonArray();
        }

        /// <summary>
        /// Sets the list to a deserialized JsonArray
        /// </summary>
        /// <param name="array"></param>
        /// <param name="serialization"></param>
        private void Deserialize(JsonArray array, EnumSerialization serialization)
        {
            List<TEnum> list = array.JsonArrayToEnumList<TEnum>(serialization);
            List<int> temp = new List<int>();
            for(int i = 0; i < list.Count; ++i)
            {
                temp.Add((int)Convert.ChangeType(list[i], typeof(int)));
            }

            this.list = temp;
        }

        /// <summary>
        /// Checks if the container is a valid enum type
        /// </summary>
        private void CheckIfEnumType()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum for EnumListContainer must be an enumerated type");
            }
        }

        /// <summary>
        /// Returns the enum type
        /// </summary>
        /// <returns></returns>
        public Type GetEnumType()
        {
            return typeof(TEnum);
        }
    }
}

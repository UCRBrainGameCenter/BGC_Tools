using System.Collections.Generic;
using BGC.Extensions;
using LightJson;
using System;
using BGC.Utility;

namespace BGC.DataStructures
{
    //@todo: When updated to C# 7.3 Remove all System Checks and add where TEnum : Enum
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

        public EnumListContainer(List<int> list) : base(list) { }
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


        public EnumListContainer(JsonArray json)
        {
            Deserialize(json);
        }

        public EnumListContainer()
        {
            list = new List<int>();
        }

        public void Add(TEnum element)
        {
            list.Add((int)Convert.ChangeType(element, typeof(int)));
        }

        public bool Remove(TEnum element)
        {
            return list.Remove((int)Convert.ChangeType(element, typeof(int)));
        }

        public bool Remove(EnumListContainer<TEnum> elc)
        {
            bool removed = true;

            for (int i = 0; i < elc.Count; ++i)
            {
                removed = Remove(elc[i]) && removed;
            }

            return removed;
        }

        public void Set(List<TEnum> list)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < list.Count; ++i)
            {
                temp.Add((int)Convert.ChangeType(list[i], typeof(int)));
            }

            this.list = temp;
        }

        public override bool Equals(object obj)
        {
            return ListExtension.ListsEquivalent(list, ((EnumListContainer<TEnum>)obj).list);
        }

        public override int GetHashCode()
        {
            return list.GetSequenceHashCode();
        }

        public bool Contains(TEnum item)
        {
            return list.Contains((int)Convert.ChangeType(item, typeof(int)));
        }

        public new JsonArray Serialize()
        {
            return list.AnyListToStringJsonArray();
        }

        public void Deserialize(JsonArray array, EnumSerialization serialization)
        {
            List<TEnum> list = array.JsonArrayToEnumList<TEnum>(serialization);
            List<int> temp = new List<int>();
            for(int i = 0; i < list.Count; ++i)
            {
                temp.Add((int)Convert.ChangeType(list[i], typeof(int)));
            }

            this.list = temp;
        }

        public void CheckIfEnumType()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum for EnumListContainer must be an enumerated type");
            }
        }

        public Type GetEnumType()
        {
            return typeof(TEnum);
        }
    }
}

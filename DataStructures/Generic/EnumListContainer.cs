using System.Collections.Generic;
using BGC.Extensions;
using UnityEngine;
using LightJson;

namespace BGC.DataStructures
{
    [System.Serializable]
    public class EnumListContainer <T>
    {
        [SerializeField]
        public List<T> list = new List<T>();

        public T this[int i]
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

        public EnumListContainer<T> Clone => new EnumListContainer<T>(new List<T>(list));
        public int RandomValue => list.RandomValue<int>();
        public int Count => list.Count;

        public EnumListContainer(List<T> list)
        {
            this.list = new List<T>(list);
        }

        public EnumListContainer(JsonArray json)
        {
            Deserialize(json);
        }

        public EnumListContainer()
        {
            list = new List<T>();
        }

        public void Add(T element)
        {
            list.Add(element);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public bool Remove(T element)
        {
            return list.Remove(element);
        }

        public bool Remove(EnumListContainer<T> elc)
        {
            bool removed = true;

            for (int i = 0; i < elc.Count; ++i)
            {
                removed = Remove(elc[i]) && removed;
            }

            return removed;
        }

        public void Set(List<T> list)
        {
            this.list = list;
        }

        public override bool Equals(object obj)
        {
            return ListExtension.ListsEquivalent(list, ((EnumListContainer<T>)obj).list);
        }

        public override int GetHashCode()
        {
            return list.GetSequenceHashCode();
        }

        public void PrintSelf()
        {
            list.PrintSelf();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public JsonArray Serialize()
        {
            return list.AnyListToStringJsonArray();
        }

        public void Deserialize(JsonArray array)
        {
            this.list = array.JsonArrayToEnumList<T>();
        }
    }
}

using System.Collections.Generic;
using BGC.Extensions;
using UnityEngine;
using LightJson;

[System.Serializable]
public class ListWrapper
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

    public ListWrapper Clone => new ListWrapper(new List<int>(list));
    public int RandomValue => list.RandomValue<int>();
    public int Count => list.Count;

    public ListWrapper(List<int> list)
    {
        this.list = new List<int>(list);
    }

    public ListWrapper(JsonArray json)
    {
        Deserialize(json);
    }

    public ListWrapper()
    {
        list = new List<int>();
    }

    public void Add(int element)
    {
        list.Add(element);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public bool Remove(int element)
    {
        return list.Remove(element);
    }

    public bool Remove(ListWrapper lw)
    {
        bool removed = true;

        for (int i = 0; i < lw.Count; ++i)
        {
            removed = Remove(lw[i]) && removed;
        }

        return removed;
    }

    public void Set(List<int> list)
    {
        this.list = list;
    }

    public override bool Equals(object obj)
    {
        return ListExtension.ListsEquivalent(list, ((ListWrapper)obj).list);
    }

    public override int GetHashCode()
    {
        return list.GetSequenceHashCode();
    }

    public void PrintSelf()
    {
        list.PrintSelf();
    }

    public bool Contains(int item)
    {
        return list.Contains(item);
    }

    public JsonArray Serialize()
    {
        return list.IntListToJsonArray();
    }

    public void Deserialize(JsonArray array)
    {
        this.list = array.JsonArrayToIntList();
    }
}
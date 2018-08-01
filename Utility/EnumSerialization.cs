using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class EnumSerialization
{
    private List<Dictionary<string, int>> enumStringValuePairs;
    private List<Dictionary<string, int>> EnumStringValuePairs
    {
        get
        {
            if (enumStringValuePairs == null)
            {
                enumStringValuePairs = new List<Dictionary<string, int>>();
            }

            return enumStringValuePairs;
        }
    }

    private Dictionary<Type, int> enumTypeIndexPair;
    private Dictionary<Type, int> EnumTypeIndexPair
    {
        get
        {
            if (enumTypeIndexPair == null)
            {
                enumTypeIndexPair = new Dictionary<Type, int>();
            }

            return enumTypeIndexPair;
        }
    }

    public bool AddEnumToDic<T>(T Enum)
    {
        if (EnumTypeIndexPair.ContainsKey(Enum.GetType()) == false)
        {
            EnumTypeIndexPair.Add(Enum.GetType(), EnumStringValuePairs.Count);
            EnumStringValuePairs.Add(new Dictionary<string, int>());
        }

        int index = EnumTypeIndexPair[Enum.GetType()];
        if (EnumStringValuePairs[index].ContainsKey(Enum.ToString()) == false)
        {
            int val;
            try
            {
                val = (int)Convert.ChangeType(Enum, typeof(int));
            }
            catch (InvalidCastException)
            {
                UnityEngine.Debug.LogError("Enum was not passed");
                val = default(int);
            }

            EnumStringValuePairs[index].Add(Enum.ToString(), val);

            return true;
        }

        return false;
    }

    public T StringToEnum<T>(string str)
    {
        Assert.IsFalse(String.IsNullOrEmpty(str));

        int index = EnumTypeIndexPair[typeof(T)];
        T val;
        try
        {
            val = (T)Convert.ChangeType(EnumStringValuePairs[index][str], typeof(T));
        }
        catch (InvalidCastException)
        {
            UnityEngine.Debug.LogError("Enum was not passed");
            val = default(T);
        }

        return val;
    }
}

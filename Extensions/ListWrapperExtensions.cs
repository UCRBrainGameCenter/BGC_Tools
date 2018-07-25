using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListWrapperExtensions
{
    /// <summary>
    /// Remove all values in array below certain count
    /// </summary>
    public static void RemoveValuesBelowValue(this ListWrapper wrapper, int value)
    {
        List<int> removeValues = new List<int>();
        for (int i = 0; i < wrapper.Count; ++i)
        {
            if (wrapper[i] < value)
            {
                removeValues.Add(wrapper[i]);
            }
        }

        for (int i = 0; i < removeValues.Count; ++i)
        {
            wrapper.Remove(removeValues[i]);
        }
    }

    /// <summary>
    /// Remove all values in array below certain count
    /// </summary>
    public static void RemoveValuesAboveValue(this ListWrapper wrapper, int value)
    {
        List<int> removeValues = new List<int>();
        for (int i = 0; i < wrapper.Count; ++i)
        {
            if (wrapper[i] > value)
            {
                removeValues.Add(wrapper[i]);
            }
        }

        for (int i = 0; i < removeValues.Count; ++i)
        {
            wrapper.Remove(removeValues[i]);
        }
    }
}

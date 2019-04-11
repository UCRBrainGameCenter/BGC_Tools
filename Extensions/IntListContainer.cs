using System.Collections.Generic;
using LightJson;
using BGC.DataStructures;

namespace BGC.Extensions
{
    public static class IntListContainerExtensions
    {
        /// <summary>
        /// Remove all values in array below certain count
        /// </summary>
        public static void RemoveValuesBelowValue(this IntListContainer container, int value)
        {
            List<int> removeValues = new List<int>();
            for (int i = 0; i < container.Count; ++i)
            {
                if (container[i] < value)
                {
                    removeValues.Add(container[i]);
                }
            }

            for (int i = 0; i < removeValues.Count; ++i)
            {
                container.Remove(removeValues[i]);
            }
        }

        /// <summary>
        /// Remove all values in array below certain count
        /// </summary>
        public static void RemoveValuesAboveValue(this IntListContainer container, int value)
        {
            List<int> removeValues = new List<int>();
            for (int i = 0; i < container.Count; ++i)
            {
                if (container[i] > value)
                {
                    removeValues.Add(container[i]);
                }
            }

            for (int i = 0; i < removeValues.Count; ++i)
            {
                container.Remove(removeValues[i]);
            }
        }

        /// <summary>
        /// Converts any JsonArray to a List of IntListContainer
        /// </summary>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<IntListContainer> JsonArrayToIntListContainerList(this JsonArray jsons)
        {
            List<IntListContainer> list = new List<IntListContainer>();
            int count = jsons.Count;

            for(int i = 0; i < count; ++i)
            {
                list.Add(new IntListContainer(jsons[i].AsJsonArray));
            }

            return list;
        }

        /// <summary>
        /// Converts a IntListContainer list to a json array of arrays
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray IntListContainerListToJsonArray(this List<IntListContainer> list)
        {
            return list.ConvertToJsonArray((IntListContainer container) =>
            {
                return container.JsonArray;
            });
        }
    }
}

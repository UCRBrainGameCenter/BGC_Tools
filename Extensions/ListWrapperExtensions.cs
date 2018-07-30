using LightJson;
using System.Collections.Generic;

namespace BGC.Extensions
{
    public static class ListWrapperExtensions
    {
        /// <summary>
        /// Remove all values in array below certain count
        /// </summary>
        public static void RemoveValuesBelowValue(this IntListContainer wrapper, int value)
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
        public static void RemoveValuesAboveValue(this IntListContainer wrapper, int value)
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

        /// <summary>
        /// Converts any JsonArray to a List of IntListContainer
        /// </summary>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<IntListContainer> JsonArrayToListListWrapper(this JsonArray jsons)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return new IntListContainer(val);
            });
        }

        /// <summary>
        /// Converts a IntListContainer list to a json array of arrays
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray ListWrapperListToJsonArray(this List<IntListContainer> list)
        {
            return list.ConvertToJsonArray((IntListContainer listWrapper) =>
            {
                return listWrapper.Serialize();
            });
        }
    }
}

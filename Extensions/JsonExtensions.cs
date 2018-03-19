using System.Collections.Generic;
using LightJson;

namespace BGC.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Converts an int list to a Json Array of Int Values
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray IntListToJsonArray(this List<int> list)
        {
            JsonArray arr = new JsonArray();
            for(int i = 0; i < list.Count; i++)
            {
                arr.Add(list[i]);
            }
            return arr;
        }

        /// <summary>
        /// Converts any list to a Json Array of String Values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray AnyListToJsonArray<T>(this List<T> list)
        {
            JsonArray arr = new JsonArray();
            for(int i = 0; i < list.Count; i++)
            {
                arr.Add(list[i].ToString());
            }
            return arr;
        }
    }
}

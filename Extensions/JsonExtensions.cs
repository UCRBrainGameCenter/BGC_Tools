using System.Collections.Generic;
using LightJson;
using System;

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
            return list.ConvertToJsonArray((int val) => {
                return new JsonValue(val);
            });
        }

        /// <summary>
        /// Converts any list to a json array of string values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray AnyListToStringJsonArray<T>(this List<T> list)
        {
            return list.ConvertToJsonArray((T val) => {
                return new JsonValue(val.ToString());
            });
        }

        /// <summary>
        /// Converts any list to a Json Array of user defined values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray ConvertToJsonArray<T>(this List<T> list, Func<T, JsonValue> convertToJsonValue)
        {
            JsonArray array = new JsonArray();
            for(int i = 0; i < list.Count; i++)
            {
                array.Add(convertToJsonValue(list[i]));
            }

            return array;
        }
    }
}

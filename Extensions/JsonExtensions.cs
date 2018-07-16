﻿using System.Collections.Generic;
using LightJson;
using System;

namespace BGC.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Converts a Json Array to a List of generic type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<int> JsonArrayToIntList(this JsonArray jsons)
        {
            List<int> list = new List<int>();

            for(int i = 0; i < jsons.Count; ++i)
            {
                list.Add(jsons[i].AsInteger);
            }

            return list;
        }

        public static List<T> JsonArrayToEnumList<T>(this JsonArray jsons)
        {
            List<T> list = new List<T>();

            for (int i = 0; i < jsons.Count; ++i)
            {
                T val = Utility.EnumUtility.StringToEnum<T>(jsons[i].AsString);
                list.Add(val);
            }

            return list;
        }

        public static JsonArray TryGetArray(this JsonObject json, string key)
        {
            if(json.ContainsKey(key) && json[key].IsJsonArray)
            {
                return json[key].AsJsonArray;
            }

            return new JsonArray();
        }

        public static JsonValue TryGetValue(this JsonObject json, string key)
        {
            if(json.ContainsKey(key) == false)
            {
                json.Add(key);
            }

            return json[key];
        }

        /// <summary>
        /// Converts an int list to a Json Array of Int Values
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray IntListToJsonArray(this List<int> list)
        {
            return list.ConvertToJsonArray((int val) => 
            {
                return new JsonValue(val);
            });
        }


        /// <summary>
        /// Converts a float list to a Json Array of float values
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray FloatListToJsonArray(this List<float> list)
        {
            return list.ConvertToJsonArray((float val) =>
            {
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

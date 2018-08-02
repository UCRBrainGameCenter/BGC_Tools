using System.Collections.Generic;
using LightJson;
using System;
using UnityEngine.Assertions;
using BGC.Utility;

namespace BGC.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Tries to get array and returns an empty array if not found
        /// </summary>
        /// <param name="json"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JsonArray TryGetArray(this JsonObject json, string key)
        {
            Assert.IsNotNull(json);
            Assert.IsFalse(string.IsNullOrEmpty(key));

            JsonArray jsonArr = json.TryGetValue(key);

            if(jsonArr == null)
            {
                return new JsonArray();
            }

            return jsonArr;
        }

        /// <summary>
        /// Try and get value, if key is not there it adds the key
        /// </summary>
        /// <param name="json"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JsonValue TryGetValue(this JsonObject json, string key)
        {
            Assert.IsNotNull(json);
            Assert.IsFalse(string.IsNullOrEmpty(key));

            if (json.ContainsKey(key) == false)
            {
                json.Add(key);
            }

            return json[key];
        }

        /// <summary>
        /// Converts a Json Array to a List of generic type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<int> JsonArrayToIntList(this JsonArray jsons)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return val.AsInteger;
            });
        }

        /// <summary>
        /// Converts a Json Array to a List of floats
        /// </summary>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<float> JsonArrayToFloatList(this JsonArray jsons)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return (float)val.AsNumber;
            });
        }

        /// <summary>
        /// Converts Any Json Array to a List of Enums
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<T> JsonArrayToEnumList<T>(this JsonArray jsons, EnumSerialization enumSerialization)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return enumSerialization.StringToEnum<T>(val.AsString);
            });
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
        /// Converts an int arr to a Json Array of Int Values
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static JsonArray IntArrayToJsonArray(this int[] arr)
        {
            JsonArray jsonArr = new JsonArray();
            for(int i = 0; i < arr.Length; ++i)
            {
                jsonArr.Add(arr[i]);
            }

            return jsonArr;
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
            return list.ConvertToJsonArray((T val) => 
            {
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

            int length = list.Count;
            for(int i = 0; i < length; i++)
            {
                array.Add(convertToJsonValue(list[i]));
            }

            return array;
        }

        /// <summary>
        /// Converts any JsonArray to List of user defined values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsons"></param>
        /// <param name="convertToObj"></param>
        /// <returns></returns>
        public static List<T> JsonArrayToList<T>(this JsonArray jsons, Func<JsonValue, T> convertToObj)
        {
            List<T> list = new List<T>();
            for(int i = 0; i < jsons.Count; ++i)
            {
                list.Add(convertToObj(jsons[i]));
            }

            return list;
        }
    }
}

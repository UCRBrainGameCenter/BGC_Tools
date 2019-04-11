using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using LightJson;
using BGC.Utility;

namespace BGC.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Tries to get array and returns an empty array if not found
        /// </summary>
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
        public static List<int> JsonArrayToIntList(this JsonArray jsonArray)
        {
            return jsonArray.JsonArrayToList((JsonValue val) =>
            {
                return val.AsInteger;
            });
        }

        public static List<string> JsonaArrayToStringList(this JsonArray jsonArray)
        {
            List<string> stringList = new List<string>();

            int count = jsonArray.Count;
            for (int i = 0; i < count; ++i)
            {
                stringList.Add(jsonArray[i]);
            }

            return stringList;
        }

        /// <summary>
        /// Converts a Json Array to a List of floats
        /// </summary>
        public static List<float> JsonArrayToFloatList(this JsonArray jsonArray)
        {
            int size = jsonArray.Count;
            List<float> floatList = new List<float>(size);

            for (int i = 0; i < size; ++i)
            {
                floatList.Add((float) jsonArray[i].AsNumber);
            }

            return floatList;
        }

        /// <summary>
        /// Converts Any Json Array to a List of Enums
        /// </summary>
        public static List<T> JsonArrayToEnumList<T>(this JsonArray jsonArray, EnumSerialization enumSerialization)
        {
            int size = jsonArray.Count;
            List<T> enumList = new List<T>(size);

            for (int i = 0; i < size; ++i)
            {
                enumList.Add(enumSerialization.StringToEnum<T>(jsonArray[i].AsString));
            }

            return enumList;
        }

        /// <summary>
        /// Converts an int list to a Json Array of Int Values
        /// </summary>
        public static JsonArray IntListToJsonArray(this IList<int> list)
        {
            int size = list.Count;
            JsonArray jsonArray = new JsonArray();

            for (int i = 0; i < size; ++i)
            {
                jsonArray.Add(list[i]);
            }

            return jsonArray;
        }

        /// <summary>
        /// Converts a float list to a Json Array of float values
        /// </summary>
        public static JsonArray FloatListToJsonArray(this IList<float> list)
        {
            int size = list.Count;
            JsonArray jsonArray = new JsonArray();

            for (int i = 0; i < size; ++i)
            {
                jsonArray.Add(list[i]);
            }

            return jsonArray;
        }

        /// <summary>
        /// Converts any list to a json array of string values
        /// </summary>
        public static JsonArray AnyListToStringJsonArray<T>(this IList<T> list)
        {
            int size = list.Count;
            JsonArray jsonArray = new JsonArray();

            for (int i = 0; i < size; ++i)
            {
                jsonArray.Add(list[i].ToString());
            }

            return jsonArray;

        }

        /// <summary>
        /// Converts any list to a Json Array of user defined values
        /// </summary>
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
        public static List<T> JsonArrayToList<T>(this JsonArray jsonArray, Func<JsonValue, T> convertToObj)
        {
            List<T> list = new List<T>();
            for(int i = 0; i < jsonArray.Count; ++i)
            {
                list.Add(convertToObj(jsonArray[i]));
            }

            return list;
        }
    }
}

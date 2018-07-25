using System.Collections.Generic;
using LightJson;
using System;

namespace BGC.Extensions
{
    public static class JsonExtensions
    {
        public static JsonArray TryGetArray(this JsonObject json, string key)
        {
            if(json == null)
            {
                json = new JsonObject();
            }

            if(json.ContainsKey(key) && json[key].IsJsonArray)
            {
                return json[key].AsJsonArray;
            }

            return new JsonArray();
        }

        public static JsonValue TryGetValue(this JsonObject json, string key)
        {
            if(json == null)
            {
                json = new JsonObject();
            }

            if(json.ContainsKey(key) == false)
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
        /// Converts Any Json Array to a List of Enums
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<T> JsonArrayToEnumList<T>(this JsonArray jsons)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return Utility.EnumUtility.StringToEnum<T>(val.AsString);
            });
        }

        /// <summary>
        /// Converts any JsonArray to a List of ListWrapper
        /// </summary>
        /// <param name="jsons"></param>
        /// <returns></returns>
        public static List<ListWrapper> JsonArrayToListListWrapper(this JsonArray jsons)
        {
            return jsons.JsonArrayToList((JsonValue val) =>
            {
                return new ListWrapper(val);
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
        /// Converts a ListWrapper list to a json array of arrays
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray ListWrapperListToJsonArray(this List<ListWrapper> list)
        {
            return list.ConvertToJsonArray((ListWrapper listWrapper) =>
            {
                return listWrapper.Serialize();
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

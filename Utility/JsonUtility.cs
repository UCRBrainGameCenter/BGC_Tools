using System.Collections.Generic;
using UnityEngine.Assertions;
using LightJson;
using System;

namespace BGC.Utility
{
    public static class JsonUtility
    {
        /// <summary>
        /// Convert list to json array with lambda that turns elements into json values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static JsonArray ToJsonArray<T>(this IList<T> list, Func<T, JsonValue> lambda)
        {
            Assert.IsNotNull(lambda);

            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < list.Count; ++i)
            {
                jsonArray.Add(lambda(list[i]));
            }

            return jsonArray;
        }

        /// <summary>
        /// Convert list of ints to json array
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JsonArray ToJsonArray(this IList<int> list)
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < list.Count; ++i)
            {
                jsonArray.Add(list[i]);
            }

            return jsonArray;
        }

        /// <summary>
        /// Convert json array to array with lambda to convert elements to defined type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonArray"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this JsonArray jsonArray, Func<JsonValue, T> lambda)
        {
            Assert.IsNotNull(lambda);
            T[] array = new T[jsonArray.Count];

            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = lambda(jsonArray[i]);
            }

            return array;
        }

        /// <summary>
        /// Conver json array to array of integers
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        public static int[] ToIntArray(this JsonArray jsonArray)
        {
            int[] array = new int[jsonArray.Count];
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = jsonArray[i];
            }

            return array;
        }

        public static JsonObject CombineJsonObjects(JsonObject a, JsonObject b)
        {
            JsonObject combined = new JsonObject();

            foreach (KeyValuePair<string, JsonValue> json in a)
            {
                combined.Add(json.Key, json.Value);
            }

            foreach (KeyValuePair<string, JsonValue> json in b)
            {
                Assert.IsFalse(combined.ContainsKey(json.Key));
                combined.Add(json.Key, json.Value);
            }

            return combined;
        }
    }
}
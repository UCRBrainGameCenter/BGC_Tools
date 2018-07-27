using System.Collections.Generic;
using UnityEngine.Assertions;
using LightJson;
using System;

namespace BGC.Utility
{
    public static class EnumUtility
    {
        /// <summary>
        /// Convert enumerations to a list, excluding any string,
        /// that when converted to lower casses, is "max"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToList<T>(bool ignoreMax = true)
        {
            List<T> list = new List<T>();

            foreach (T t in Enum.GetValues(typeof(T)))
            {
                if (ignoreMax == true && t.ToString().ToLower().Equals("max") == false)
                {
                    list.Add(t);
                }
            }

            return list;
        }

        /// <summary>
        /// Convert json boject to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prependString">Set this if you want there be a value before every key</param>
        /// <param name="ignoreMax">Set this to true to not add any key that is max</param>
        /// <returns></returns>
        public static JsonObject ToJsonObject<T>(string prependString = "", bool ignoreMax = true)
        {
            Assert.IsNotNull(prependString);

            JsonObject jo = new JsonObject();
            Array enumValues = Enum.GetValues(typeof(T));

            int length = enumValues.Length;
            for(int i = 0; i < length; ++i)
            {
                T t = (T) enumValues.GetValue(i);
                string tName = t.ToString();

                if (ignoreMax == true && tName.ToLower().Equals("max"))
                {
                    continue;
                }

                jo.Add($"{prependString}{((int)(object)t).ToString()}", tName);
            }

            return jo;
        }
    }
}
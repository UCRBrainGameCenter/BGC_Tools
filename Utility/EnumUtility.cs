using System;
using System.Collections.Generic;
using LightJson;

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

        public static JsonObject ToJsonObject<T>(bool ignoreMax = true)
        {
            JsonObject jo = new JsonObject();

            foreach (T t in Enum.GetValues(typeof(T)))
            {
                if (ignoreMax == true && t.ToString().ToLower().Equals("max") == false)
                {
                    jo.Add(((int)(object)t).ToString(), t.ToString());
                }
            }

            return jo;
        }
    }
}
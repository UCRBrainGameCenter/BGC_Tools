using System;
using System.Collections.Generic;

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
                if (ignoreMax == true && t.ToString().ToLower() != "max")
                {
                    list.Add(t);
                }
            }

            return list;
        }
    }
}
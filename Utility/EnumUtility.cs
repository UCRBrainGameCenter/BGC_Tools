using System;
using System.Collections.Generic;
using LightJson;

namespace BGC.Utility
{
    public static class EnumUtility
    {
        private static List<Dictionary<string, int>> enumStringValuePairs;
        private static List<Dictionary<string, int>> EnumStringValuePairs
        {
            get
            {
                if(enumStringValuePairs == null)
                {
                    enumStringValuePairs = new List<Dictionary<string, int>>();
                }

                return enumStringValuePairs;
            }
        }

        private static Dictionary<Type, int> enumTypeIndexPair;
        private static Dictionary<Type, int> EnumTypeIndexPair
        {
            get
            {
                if(enumTypeIndexPair == null)
                {
                    enumTypeIndexPair = new Dictionary<Type, int>();
                }

                return enumTypeIndexPair;
            }
        }

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

        public static bool AddEnumToDic<T>(T Enum)
        {
            if(EnumTypeIndexPair.ContainsKey(Enum.GetType()) == false)
            {
                EnumTypeIndexPair.Add(Enum.GetType(), EnumStringValuePairs.Count);
                EnumStringValuePairs.Add(new Dictionary<string, int>());
            }

            int index = EnumTypeIndexPair[Enum.GetType()];
            if(EnumStringValuePairs[index].ContainsKey(Enum.ToString()) == false)
            {
                int val;
                try
                {
                    val = (int)Convert.ChangeType(Enum, typeof(int));
                }
                catch(InvalidCastException)
                {
                    UnityEngine.Debug.LogError("Enum was not passed");
                    val = default(int);
                }

                EnumStringValuePairs[index].Add(Enum.ToString(), val);

                return true;
            }

            return false;
        }

        public static T StringToEnum<T>(string str)
        {
            int index = EnumTypeIndexPair[typeof(T)];
            T val;
            try
            {
                val = (T)Enum.Parse(typeof(T), str);
            }
            catch(InvalidCastException)
            {
                UnityEngine.Debug.LogError("Enum was not passed");
                val = default(T);
            }
            catch(ArgumentNullException)
            {
                UnityEngine.Debug.LogWarning("String Empty");
                val = default(T);
            }

            return val;
        }
    }
}
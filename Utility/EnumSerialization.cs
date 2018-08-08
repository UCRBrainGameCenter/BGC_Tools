using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BGC.Utility
{
    public class EnumSerialization
    {
        private List<Dictionary<string, int>> enumStringValuePairs;
        private List<Dictionary<string, int>> EnumStringValuePairs
        {
            get
            {
                if (enumStringValuePairs == null)
                {
                    enumStringValuePairs = new List<Dictionary<string, int>>();
                }

                return enumStringValuePairs;
            }
        }

        private Dictionary<Type, int> enumTypeIndexPair;
        private Dictionary<Type, int> EnumTypeIndexPair
        {
            get
            {
                if (enumTypeIndexPair == null)
                {
                    enumTypeIndexPair = new Dictionary<Type, int>();
                }

                return enumTypeIndexPair;
            }
        }

        /// <summary>
        /// Adds an Enum Value to serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Enum"></param>
        /// <returns></returns>
        public bool AddEnumToDic<T>(T Enum)
        {
            if (EnumTypeIndexPair.ContainsKey(Enum.GetType()) == false)
            {
                EnumTypeIndexPair.Add(Enum.GetType(), EnumStringValuePairs.Count);
                EnumStringValuePairs.Add(new Dictionary<string, int>());
            }

            int index = EnumTypeIndexPair[Enum.GetType()];
            if (EnumStringValuePairs[index].ContainsKey(Enum.ToString()) == false)
            {
                int val;
                try
                {
                    val = (int)Convert.ChangeType(Enum, typeof(int));
                }
                catch (InvalidCastException)
                {
                    UnityEngine.Debug.LogError("Enum was not passed");
                    val = default(int);
                }

                EnumStringValuePairs[index].Add(Enum.ToString(), val);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a String to an Enum that was added to the serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public T StringToEnum<T>(string str)
        {
            Assert.IsFalse(String.IsNullOrEmpty(str));

            int index = EnumTypeIndexPair[typeof(T)];
            T val;
            try
            {
                val = (T)Enum.ToObject(typeof(T), EnumStringValuePairs[index][str]);
            }
            catch (InvalidCastException)
            {
                UnityEngine.Debug.LogError("Enum was not passed");
                val = default(T);
            }

            return val;
        }
    }
}

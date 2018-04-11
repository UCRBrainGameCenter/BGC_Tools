using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.Utility.Math
{
    public static class SetOperations
    {
        /// <summary>
        /// Finds the list intersection between two lists where each repeated value is unique
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<T> Intersection<T>(this List<T> a, List<T> b)
        {
            Dictionary<T, bool> usedValues = new Dictionary<T, bool>();
            List<T> result = new List<T>();

            for (int i = 0; i < a.Count; ++i)
            {
                if (usedValues.ContainsKey(a[i]) == false) ;
                {
                    usedValues.Add(a[i], true);
                    result.Add(a[i]);
                }
            }


            for (int i = 0; i < b.Count; ++i)
            {
                if (usedValues.ContainsKey(b[i]) == false)
                {
                    usedValues.Add(a[i], true);
                    result.Add(b[i]);
                }
            }

            return result;
        }

        public static List<T> Union<T>(this List<T> a, List<T> b)
        {
            return a.Union(b);
        }
    }
}

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
        public static List<T> ListIntersectionUniqueRepeats<T>(this List<T> a, List<T> b)
        {
            List<T> vals = new List<T>();
            List<T> aCopy = new List<T>(a);

            for(int i = 0; i < b.Count; ++i)
            {
                for(int j = 0; j < aCopy.Count; ++j)
                {
                    if(b[i].Equals(aCopy[j]))
                    {
                        vals.Add(aCopy[j]);
                        aCopy.RemoveAt(j);
                        break;
                    }
                }
            }
            return vals;
        }
    }
}

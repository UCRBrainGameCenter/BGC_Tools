using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BGC.Extensions
{
    /// <summary>
    /// Set of extensions for a list for easier use of ILists
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        /// Get a random index from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int RandomIndex(this IList list)
        {
            if (list.Count <= 0)
            {
                return -1;
            }

            return Random.Range(0, list.Count);
        }

        /// <summary>
        /// Get a random value from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomValue<T>(this IList list)
        {
            if (list.Count == 0)
            {
                Debug.LogError(
                    "Received list of length 0 which doesn't allow for random value, " +
                    "returning default value");
                return default(T);
            }

            return (T)list[list.RandomIndex()];
        }

        /// <summary>
        /// Randomize a list
        /// </summary>
        /// <param name="list"></param>
        public static void Shuffle(this IList list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                int randomIndex = Random.Range(i, list.Count);
                object temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Get last index of the list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int LastIndex(this IList list)
        {
            return list.Count - 1;
        }

        /// <summary>
        /// Get the last element in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T LastVal<T>(this IList list)
        {
            return (T)list[list.LastIndex()];
        }

        /// <summary>
        /// swap elements in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="indexA"></param>
        /// <param name="indexB"></param>
        public static void Swap<T>(this IList list, int indexA, int indexB)
        {
            T tmp = (T)list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        /// <summary>
        /// Test if list a and list b are exactly equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool ListsEquivalent<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i].Equals(b[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/8094867/good-gethashcode-override-for-list-of-foo-objects-respecting-the-order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static int GetSequenceHashCode<T>(this IList<T> sequence)
        {
            unchecked
            {
                int hash = 19;
                foreach (var foo in sequence)
                {
                    hash = hash * 31 + foo.GetHashCode();
                }

                return hash;
            }
        }
    }
}
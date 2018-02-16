﻿using System.Collections.Generic;
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

        /// <summary>
        /// Get a list of all the valid indexes in the list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int[] Indexes(this IList list)
        {
            int[] indexes = new int[list.Count];

            for (int i = 0; i < list.Count; ++i)
            {
                indexes[i] = i;
            }

            return indexes;
        }

        /// <summary>
        /// Add an item to a list if it is not already in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns>True if the item was not present and added to the list</returns>
        public static bool SetAdd<T>(this List<T> list, T item)
        {
            bool added = false;

            if (list.Contains(item) == false)
            {
                list.Add(item);
                added = true;
            }

            return added;
        }

        /// <summary>
        /// Remove all occurrences of item in list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns>if item was found at all in list and removed</returns>
        public static bool SetSub<T>(this List<T> list, T item)
        {
            bool removed = false;

            while (list.Contains(item))
            {
                list.Remove(item);
                removed = true;
            }

            return removed;
        }

        /// <summary>
        /// Get the maximum value of a list. Best to only use with a list of numbers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static T Max<T>(this List<T> list, bool verbose = false)
        {
            if (list.Count <= 0)
            {
                if (verbose)
                {
                    Debug.LogError("List size is less than or equal to 0.");
                }

                return default(T);
            }

            T max = list[0];
            for (int i = 1; i < list.Count; ++i)
            {
                if (Comparer<T>.Default.Compare(max, list[i]) < 0)
                {
                    max = list[i];
                }
            }

            return max;
        }

        /// <summary>
        /// Get the minimum value of a list. Best to only use with a list of numbers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static T Min<T>(this List<T> list, bool verbose = false)
        {
            if (list.Count <= 0)
            {
                if (verbose)
                {
                    Debug.LogError("List size is less than or equal to 0.");
                }

                return default(T);
            }

            T min = list[0];
            for (int i = 1; i < list.Count; ++i)
            {
                if (Comparer<T>.Default.Compare(min, list[i]) > 0)
                {
                    min = list[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Create a shallow copy of the list
        /// 
        /// src: https://stackoverflow.com/questions/222598/how-do-i-clone-a-generic-list-in-c
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> ShallowClone<T>(this List<T> list)
        {
            List<T> newList = new List<T>(list.Capacity);
            newList.AddRange(list);

            return newList;
        }

        /// <summary>
        /// Debug.Log to print out all values of the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void PrintSelf<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                Debug.Log(i + ") " + list[i]);
            }
        }
    }
}
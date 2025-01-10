using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

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
                return default;
            }

            return (T)list[list.RandomIndex()];
        }

        /// <summary>
        /// Get a random value from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="excludeIndicies"></param>
        /// <returns></returns>
        public static T RandomValue<T>(this IList list, params int[] excludeIndicies)
        {
            int length = list.Count;
            if (length == 0)
            {
                Debug.LogError(
                    "Received list of length 0 which doesn't allow for random value, " +
                    "returning default value");
                return default;
            }

            List<int> indexes = new List<int>();
            for (int i = 0; i < length; ++i)
            {
                if (excludeIndicies.Contains(i) == false)
                {
                    indexes.Add(i);
                }
            }

            if (indexes.Count == 0)
            {
                Debug.LogError(
                    "Recieved array of excludedIndicies that does not allow for any values to be returned, " +
                    "returning default value");

                return default;
            }

            return (T)list[indexes.RandomValue<int>()];
        }

        /// <summary>
        /// Randomize a list
        /// </summary>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int length = list.Count - 1;
            for (int i = 0; i < length; ++i)
            {
                int randomIndex = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Randomize an enumerable and return it as an array.
        /// </summary>
        /// <param name="source"></param>
        public static T[] Shuffled<T>(this IEnumerable<T> source)
        {
            T[] array = source.ToArray();
            array.Shuffle();
            return array;
        }

        /// <summary>
        /// Randomize a list
        /// </summary>
        /// <param name="list"></param>
        public static void Shuffle(this IList list, System.Random randomizer)
        {
            int length = list.Count;
            for (int i = 0; i < length; ++i)
            {
                int randomIndex = randomizer.Next(i, list.Count);
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
            int length = a.Count;
            if (length != b.Count)
            {
                return false;
            }

            for (int i = 0; i < length; ++i)
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

            int length = list.Count;
            for (int i = 0; i < length; ++i)
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

                return default;
            }

            T max = list[0];
            int length = list.Count;
            for (int i = 1; i < length; ++i)
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

                return default;
            }

            T min = list[0];
            int length = list.Count;
            for (int i = 1; i < length; ++i)
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
        /// Debug.Log to print out all values of the lists=
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void PrintSelf<T>(this List<T> list)
        {
            int length = list.Count;
            for (int i = 0; i < length; ++i)
            {
                Debug.Log($"{i}) {list[i]}");
            }
        }

        /// <summary>
        /// Finds all instances of an object within a list and returns them
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<T> FindAllInstancesOf<T>(this List<T> list, T value)
        {
            List<T> instances = new List<T>();

            int length = list.Count;
            for (int i = 0; i < length; ++i)
            {
                if (list[i].Equals(value))
                {
                    instances.Add(list[i]);
                }
            }

            return instances;
        }

        /// <summary>
        /// Join a list into a string with a separator of the users choice
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="separator">defaults to a comma</param>
        /// <returns></returns>
        public static string Join<T>(this List<T> list, string separator = ",")
        {
            Assert.IsNotNull(list);
            Assert.IsFalse(string.IsNullOrEmpty(separator));

            int length = list.Count;
            string result = "";

            if (length != 0)
            {
                result = list[0].ToString();

                for (int i = 1; i < length; ++i)
                {
                    result = $"{result}{separator}{list[i]}";
                }
            }

            return result;
        }

        /// <summary>
        ///  Join an array into a string with a separator of the users choice
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join<T>(this T[] arr, string separator)
        {
            int length = arr.Length;
            if (length == 0)
            {
                return "";
            }

            string result = arr[0].ToString();
            for (int i = 1; i < length; ++i)
            {
                result = $"{result}{separator}{arr[i]}";
            }

            return result;
        }

        /// <summary>
        /// Attempts to get an element from an array and returns it. If not, returns default.
        /// Outputs the result to the out parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetElement<T>(this List<T> list, int index, out T value)
        {
            Assert.IsNotNull(list);
            bool result = false;

            if (index < list.Count && index > -1)
            {
                value = list[index];
                result = true;
            }
            else
            {
                value = default;
            }

            return result;
        }

        /// Checks if the list contains the defined value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="val"></param>
        /// <returns>True if the list has the defined value</returns>
        public static bool Conains<T>(IList list, T val)
        {
            bool found = false;
            int length = list.Count;
            for (int i = 0; i < length; ++i)
            {
                if (list[i].Equals(val))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Rearranges the list such that no two consecutive elements
        /// follow their natural sorted order.
        /// Uses the default comparer for type T to determine the natural order.
        /// If no valid rearrangement is possible, the list remains unchanged.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to be anti-sorted.</param>
        public static void AntiSort<T>(this List<T> list)
        {
            // Use the default comparer for T
            list.AntiSort(Comparer<T>.Default);
        }

        /// <summary>
        /// Rearranges the list such that no two consecutive elements
        /// follow their order as defined by the provided comparison.
        /// If no valid rearrangement is possible, the list remains unchanged.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to be anti-sorted.</param>
        /// <param name="comparison">
        /// The comparison used to determine the sorted order of elements.
        /// </param>
        public static void AntiSort<T>(this List<T> list, System.Comparison<T> comparison)
        {
            list.AntiSort(Comparer<T>.Create(comparison));
        }

        /// <summary>
        /// Rearranges the list such that no two consecutive elements
        /// follow their order as defined by the provided comparer.
        /// If no valid rearrangement is possible, the list remains unchanged.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to be anti-sorted.</param>
        /// <param name="comparer">
        /// The comparer used to determine the sorted order of elements.
        /// </param>
        public static void AntiSort<T>(this List<T> list, IComparer<T> comparer)
        {
            // Cannot anti-sort a list with less than 2 elements
            if (list.Count < 2)
            {
                return;
            }

            // Sort the list so we know which pairs cannot appear next to each other
            List<T> sortedList = new(list);
            sortedList.Sort(comparer);

            // Create a map of each item to the next item
            // NOTE: Cannot use Dictionary<T, T> because we cannot trust hash codes due to the provided comparer
            SortedDictionary<T, T> nextItem = new(comparer);
            for (int i = 0; i < sortedList.Count - 1; i++)
            {
                T item = sortedList[i];
                T next = sortedList[i + 1];

                // Use the last instance of an item so that the next item isn't just the next instance of the same item
                if (comparer.Compare(item, next) != 0)
                {
                    nextItem[item] = next;
                }
            }

            // Use a stack for DFS; each element is (current pattern, remaining values, next index into remaining values)
            List<(List<T>, List<T>, int)> stack = new()
            {
                (new List<T>(), new List<T>(list), 0)
            };

            while (stack.Count > 0)
            {
                var (pattern, remainingValues, nextRemainingValuesindex) = stack[^1];
                stack.RemoveAt(stack.Count - 1);

                bool hasBannedNext = pattern.Count > 0 && nextItem.ContainsKey(pattern[^1]);
                T bannedNext = hasBannedNext ? nextItem[pattern[^1]] : default;
                for (int i = nextRemainingValuesindex; i < remainingValues.Count; i++)
                {
                    T candidate = remainingValues[i];

                    // If the candidate equals the latest item in the pattern, skip it
                    if (pattern.Count > 0 && comparer.Compare(candidate, pattern[^1]) == 0)
                    {
                        continue;
                    }

                    // If the candidate is the banned next item, skip it
                    if (hasBannedNext && comparer.Compare(candidate, bannedNext!) == 0)
                    {
                        continue;
                    }

                    // If there was only one remaining value, then we have found a valid list and are done
                    bool isDone = remainingValues.Count == 1;

                    // For continuing where this loop left off after backtracking
                    if (!isDone && i + 1 < remainingValues.Count)
                    {
                        stack.Add((pattern, remainingValues, i + 1));
                    }

                    List<T> newPattern = pattern.Append(candidate).ToList();
                    if (isDone)
                    {
                        list.Clear();
                        list.AddRange(newPattern);
                        return;
                    }

                    // Create a new remaining values list without this specific candidate instance
                    List<T> newRemainingValues = new(remainingValues);
                    newRemainingValues.RemoveAt(i);

                    stack.Add((newPattern, newRemainingValues, 0));
                    break;
                }
            }
        }
    }
}
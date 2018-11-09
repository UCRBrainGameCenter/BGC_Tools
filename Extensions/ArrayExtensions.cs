using UnityEngine;
using UnityEngine.Assertions;

namespace BGC.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Get a random index of the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int RandomIndex<T>(this T[] array)
        {
            return Random.Range(0, array.Length);
        }

        /// <summary>
        /// Get a random value from the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T RandomValue<T>(this T[] array)
        {
            return array[array.RandomIndex()];
        }

        /// <summary>
        /// Check if an array contains a target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Contains<T>(this T[] array, T target)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(target))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// add element to array. Extends array length by 1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="item"></param>
        public static T[] Add<T>(this T[] array, T item)
        {
            T[] newArray = new T[array.Length + 1];
            newArray[array.Length] = item;

            for (int i = 0; i < array.Length; ++i)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }

        /// <summary>
        /// Get a Range of Elements in an Array.
        /// endIndex is NOT INCLUSIVE
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static T[] GetRange<T>(this T[] array, int startIndex, int endIndex = -1)
        {
            if (endIndex == -1)
            {
                endIndex = array.Length;
            }

            Assert.IsNotNull(array);
            Assert.IsTrue(startIndex > -1);
            Assert.IsTrue(endIndex >= startIndex);
            Assert.IsFalse(startIndex > array.Length);
            Assert.IsFalse(endIndex > array.Length);

            T[] arr = new T[endIndex - startIndex];
            for (int i = startIndex; i < endIndex; ++i)
            {
                arr[i - startIndex] = array[i];
            }

            return arr;
        }

        /// <summary>
        /// Gets an array of all the indexes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int[] Indexes<T>(this T[] arr)
        {
            int[] indexes = new int[arr.Length];

            for (int i = 0; i < arr.Length; ++i)
            {
                indexes[i] = i;
            }

            return indexes;
        }

        /// <summary>
        /// Search a (ordered) float array for the first value exceeding or equal to the argument
        /// </summary>
        public static int BinarySearchLowerBound(this float[] array, float value)
        {
            int min = 0;
            int max = array.Length;
            int guess;

            if (value < array[0])
            {
                return -1;
            }

            while (true)
            {
                guess = (max + min) / 2;

                if (guess == min)
                {
                    return guess;
                }
                else if (array[guess] == value)
                {
                    return guess;
                }
                else if (value > array[guess])
                {
                    min = guess;
                }
                else
                {
                    max = guess;
                }
            }
        }
    }
}
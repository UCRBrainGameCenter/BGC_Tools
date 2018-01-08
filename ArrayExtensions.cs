using UnityEngine;

namespace BGCTools
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
    }
}
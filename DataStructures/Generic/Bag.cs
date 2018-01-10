using System.Collections.Generic;
using BGC.Extensions;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// Implementation of bag data structure. Refer to https://algs4.cs.princeton.edu/13stacks/
    /// for notes on how it suspposed to function and how this class can be improved 
    ///
    /// \remark{This class is okay but not perfect. My particular issue with it is 
    //        that I cannot clone the templates given. Cloning the templates 
    //        would be ideal because we can keep the clean list safe from outside
    //        damage through modification. Inherently the code is less safe with
    //        this data structure if the programmer is not paying attention and 
    //        making sure to program in cloning after a Pull and also not modifying
    //        the passed in list.}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Bag<T>
    {
        private List<T> cleanList = new List<T>();
        private List<T> bag       = new List<T>();

        /// <summary>
        /// Constructor method for bag with a list to build from
        /// </summary>
        /// <param name="startingCleanList"></param>
        public Bag(List<T> startingCleanList)
        {
            for (int i = 0; i < startingCleanList.Count; ++i)
            {
                cleanList.Add(startingCleanList[i]);
            }

            Reset();
        }

        /// <summary>
        /// Constructor method for a bag with an array to build from
        /// </summary>
        /// <param name="startingCleanList"></param>
        public Bag(T[] startingCleanList)
        {
            for (int i = 0; i < startingCleanList.Length; ++i)
            {
                cleanList.Add(startingCleanList[i]);
            }

            Reset();
        }

        /// <summary>
        /// Add element to the bag
        /// </summary>
        /// <param name="element"></param>
        public void Add(T element)
        {
            cleanList.Add(element);
        }

        /// <summary>
        /// add element to clean list and current bag
        /// </summary>
        /// <param name="element"></param>
        public void AddToBoth(T element)
        {
            cleanList.Add(element);
            bag.Add(element);
        }

        /// <summary>
        /// Rese tbag to have entire clean list
        /// </summary>
        public void Reset()
        {
            bag = new List<T>(cleanList);
        }

        /// <summary>
        /// Pull from the bag and reset if autoreset is set to true else receive default value.
        /// </summary>
        /// <param name="autoReset"></param>
        /// <returns></returns>
        public T Pull(bool autoReset = false)
        {
            T element;

            if (IsEmpty() == false)
            {
                int index = bag.RandomIndex();
                element = bag[index];
                bag.RemoveAt(index);

                if (autoReset == true && IsEmpty() == true)
                {
                    Reset();
                }
            }
            else
            {
                // @note: the state where (autoReset == true && cleanList.Count > 0) == true 
                //        is impossible with the current implementation. If it is, then there
                //        should be an extra if else here where that statement is evaluated and
                //        if true then the Reset() functionchould be called and then element 
                //        should be set with a recursive Pull(autoReset) call.
                element = default(T);
            }

            return element;
        }

        /// <summary>
        /// Returns whether or not the bag is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return bag.Count <= 0;
        }
    }
}

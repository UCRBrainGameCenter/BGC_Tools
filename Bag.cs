using System.Collections.Generic;

namespace BGCTools
{
    // @note: this class is okay but not perfect. My particular issue with it is 
    //        that I cannot clone the templates given. Cloning the templates 
    //        would be ideal because we can keep the clean list safe from outside
    //        damage through modification. Inherently the code is less safe with
    //        this data structure if the programmer is not paying attention and 
    //        making sure to program in cloning after a Pull and also not modifying
    //        the passed in list.
    public class Bag<T>
    {
        private List<T> cleanList = new List<T>();
        private List<T> bag       = new List<T>();

        public Bag(List<T> startingCleanList)
        {
            for (int i = 0; i < startingCleanList.Count; ++i)
            {
                cleanList.Add(startingCleanList[i]);
            }

            Reset();
        }

        public void Add(T element)
        {
            cleanList.Add(element);
        }

        public void AddToBoth(T element)
        {
            cleanList.Add(element);
            bag.Add(element);
        }

        public void Reset()
        {
            bag = new List<T>(cleanList);
        }

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

        public bool IsEmpty()
        {
            return bag.Count <= 0;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// A depletable/refillable set with an underlying list and defined order.
    /// </summary>
    public class DepletableList<T> : IDepletable<T>
    {
        protected List<T> values;
        protected List<bool> valueDepleted;

        protected int currentIndex;

        public DepletableList()
        {
            currentIndex = 0;

            values = new List<T>();
            valueDepleted = new List<bool>();
        }

        public DepletableList(IEnumerable<T> values, bool autoRefill = false)
        {
            currentIndex = 0;

            this.values = new List<T>(values);
            AutoRefill = autoRefill;

            valueDepleted = new List<bool>(this.values.Count);
        }

        private int GetRemainingCount()
        {
            int remainingCount = 0;

            //Advance in case the remaining have been depleted and we should refill
            AdvanceCurrentIndexPastDepleted();

            for (int i = currentIndex; i < values.Count; i++)
            {
                if (!valueDepleted[i])
                {
                    ++remainingCount;
                }
            }

            return remainingCount;
        }

        #region IDepletable<T>

        public bool AutoRefill { get; set; }
        public int TotalCount => values.Count;

        private void AdvanceCurrentIndexPastDepleted(bool allowRefill = true)
        {
            while (currentIndex < values.Count && valueDepleted[currentIndex])
            {
                currentIndex++;
            }

            if (allowRefill && AutoRefill && currentIndex >= values.Count)
            {
                Reset();
                AdvanceCurrentIndexPastDepleted(allowRefill: false);
            }
        }


        public T PopNext()
        {
            if (values.Count == 0)
            {
                Debug.LogWarning("List is empty and you tried to draw an element.");
                return default;
            }

            AdvanceCurrentIndexPastDepleted();

            if (currentIndex >= values.Count)
            {
                Debug.LogWarning("List is depleted and you tried to draw an element.");
                return default;
            }

            valueDepleted[currentIndex] = true;

            return values[currentIndex++];
        }

        public bool TryPopNext(out T value)
        {
            if (values.Count == 0)
            {
                value = default;
                return false;
            }

            AdvanceCurrentIndexPastDepleted();

            if (currentIndex >= values.Count)
            {
                value = default;
                return false;
            }

            valueDepleted[currentIndex] = true;
            value = values[currentIndex++];
            return true;
        }

        /// <summary>
        /// Fills the bag back up.
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
            for (int i = 0; i < valueDepleted.Count; i++)
            {
                valueDepleted[i] = false;
            }
        }

        public bool DepleteValue(T value)
        {
            const int NOT_FOUND = -1;
            int index = NOT_FOUND;

            for (int i = currentIndex; i < values.Count; i++)
            {
                if (values[i].Equals(value))
                {
                    index = i;
                    break;
                }
            }

            if (index == NOT_FOUND)
            {
                return false;
            }

            valueDepleted[index] = true;
            return true;
        }

        public bool DepleteAllValue(T value)
        {
            bool success = false;

            while (DepleteValue(value))
            {
                success = true;
            }

            return success;
        }

        public bool ReplenishValue(T value)
        {
            const int NOT_FOUND = -1;
            int index = NOT_FOUND;

            for (int i = currentIndex - 1; i >= 0; i--)
            {
                if (values[i].Equals(value))
                {
                    index = i;
                    break;
                }
            }

            if (index == NOT_FOUND)
            {
                return false;
            }

            valueDepleted[index] = false;
            //Jump the currentIndex back to the restored value
            currentIndex = index;
            return true;
        }

        public bool ReplenishAllValue(T value)
        {
            bool success = false;

            while (ReplenishValue(value))
            {
                success = true;
            }

            return success;
        }

        public bool ContainsAnywhere(T value) => values.Contains(value);

        public IList<T> GetAvailable()
        {
            List<T> availableValues = new List<T>();

            //Advance, just in case the remainder have all been targeted for depletion
            AdvanceCurrentIndexPastDepleted();

            for (int i = currentIndex; i < values.Count; i++)
            {
                if (!valueDepleted[i])
                {
                    availableValues.Add(values[i]);
                }
            }

            return availableValues;
        }

        public void CopyAllTo(T[] array, int arrayIndex)
        {
            values.CopyTo(
                index: 0,
                array: array,
                arrayIndex: arrayIndex,
                count: Math.Min(values.Count, array.Length - arrayIndex));
        }

        #endregion IDepletable<T>
        #region ICollection<T>

        public int Count => GetRemainingCount();

        bool ICollection<T>.IsReadOnly => false;

        public void Add(T value)
        {
            //Advance, just in case the remainder have all been targeted for depletion
            AdvanceCurrentIndexPastDepleted();

            values.Add(value);
            valueDepleted.Add(false);
        }

        public void Clear()
        {
            currentIndex = 0;
            values.Clear();
            valueDepleted.Clear();
        }

        public bool Contains(T value)
        {
            AdvanceCurrentIndexPastDepleted();

            for (int i = currentIndex; i < values.Count; i++)
            {
                if (values[i].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] dest, int destIndex)
        {
            AdvanceCurrentIndexPastDepleted();

            values.CopyTo(
                index: currentIndex,
                array: dest,
                arrayIndex: destIndex,
                count: Math.Min(values.Count - currentIndex, dest.Length - destIndex));
        }

        public bool Remove(T item)
        {
            int index = values.IndexOf(item);

            if (index > -1)
            {
                values.RemoveAt(index);
                valueDepleted.RemoveAt(index);

                //Roll back currentIndex if its element just shifted leftward
                if (currentIndex > index)
                {
                    --currentIndex;
                }
            }

            return index != -1;
        }

        #endregion ICollection<T>
        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator() => values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();

        #endregion IEnumerable<T>
    }
}
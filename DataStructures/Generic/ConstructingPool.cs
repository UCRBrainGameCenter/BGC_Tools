using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BGC.DataStructures.Generic
{
    public sealed class ConstructingPool<T> : IPool<T>
    {
        private readonly HashSet<T> available;
        private readonly HashSet<T> checkedOut;

        private readonly Func<T> itemConstructor;

        public delegate void ItemModifier(T item);

        public ItemModifier onCreate = null;
        public ItemModifier onCheckOut = null;
        public ItemModifier onCheckIn = null;

        private T NewItem
        {
            get
            {
                T item = itemConstructor();
                onCreate?.Invoke(item);
                return item;
            }
        }

        #region IPool<T>

        public int TotalCount => available.Count + checkedOut.Count;

        public ConstructingPool(Func<T> itemConstructor)
        {
            available = new HashSet<T>();
            checkedOut = new HashSet<T>();

            this.itemConstructor = itemConstructor;
        }

        public ConstructingPool(IEnumerable<T> values, Func<T> itemConstructor)
        {
            available = new HashSet<T>(values);
            checkedOut = new HashSet<T>();

            this.itemConstructor = itemConstructor;
        }

        public void Populate(int itemCount)
        {
            int targetCount = itemCount - available.Count;

            for (int i = 0; i < targetCount; i++)
            {
                available.Add(NewItem);
            }
        }

        public T CheckOut()
        {
            T value;
            if (available.Count == 0)
            {
                value = NewItem;
            }
            else
            {
                //Create new item
                value = available.First();
                available.Remove(value);
            }

            checkedOut.Add(value);
            onCheckOut?.Invoke(value);
            return value;
        }

        public T CheckOut(Func<T, bool> predicate)
        {
            T value = available.FirstOrDefault(predicate);

            if (value.Equals(default(T)))
            {
                value = NewItem;
            }
            else
            {
                available.Remove(value);
            }

            checkedOut.Add(value);
            onCheckOut?.Invoke(value);
            return value;
        }

        public bool TryCheckOut(out T value)
        {
            if (available.Count == 0)
            {
                value = default;
                return false;
            }

            value = available.First();
            available.Remove(value);
            checkedOut.Add(value);
            onCheckOut?.Invoke(value);
            return true;
        }

        public bool TryCheckOut(Func<T, bool> predicate, out T value)
        {
            value = available.FirstOrDefault(predicate);

            if (value.Equals(default(T)))
            {
                return false;
            }

            available.Remove(value);
            checkedOut.Add(value);
            onCheckOut?.Invoke(value);
            return true;
        }

        public void CheckIn(T value)
        {
            onCheckIn?.Invoke(value);
            available.Add(value);
            checkedOut.Remove(value);
        }

        public void CheckInAll()
        {
            if (onCheckIn != null)
            {
                foreach (var value in checkedOut)
                {
                    onCheckIn(value);
                }
            }
            available.UnionWith(checkedOut);
            checkedOut.Clear();
        }

        public bool ContainsAnywhere(T value) => available.Contains(value) || checkedOut.Contains(value);

        public void CopyAllTo(T[] array, int arrayIndex)
        {
            available.CopyTo(array, arrayIndex);
            checkedOut.CopyTo(array, arrayIndex + available.Count);
        }

        public IEnumerable<T> Available => available;

        public IEnumerable<T> CheckedOut => checkedOut;

        public IEnumerable<T> GetAvailable(Func<T, bool> predicate) => available.Where(predicate);
        
        public IEnumerable<T> GetCheckedOut(Func<T, bool> predicate) => checkedOut.Where(predicate);

        public bool AvailableContains(T value) => available.Contains(value);

        public bool CheckedOutContains(T value) => checkedOut.Contains(value);

        /// <summary>
        /// This does not call the check out/check in delegates.
        /// </summary>
        public void MarkedCheckedIn(T value)
        {
            checkedOut.Remove(value);
            available.Add(value);
        }

        /// <summary>
        /// This does not call the check out/check in delegates.
        /// </summary>
        public void MarkedCheckedOut(T value)
        {
            available.Remove(value);
            checkedOut.Add(value);
        }

        #endregion IPool<T>
        #region IPoolRelease<T>

        void IPoolRelease<T>.Release(T value)
        {
            if (!checkedOut.Remove(value))
            {
                UnityEngine.Debug.LogError($"Released an Item that wasn't CheckedOut: {value}");
                return;
            }

            available.Add(value);
            onCheckIn?.Invoke(value);
        }

        #endregion IPoolRelease<T>
        #region ICollection<T>

        public int Count => available.Count;

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            available.Add(item);
            checkedOut.Remove(item);
            onCheckIn?.Invoke(item);
        }

        void ICollection<T>.Clear()
        {
            available.Clear();
            checkedOut.Clear();
        }

        bool ICollection<T>.Contains(T item) => available.Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => available.CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item) => available.Remove(item) || checkedOut.Remove(item);

        #endregion ICollection<T>
        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator() => available.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => available.GetEnumerator();

        #endregion IEnumerable<T>
    }

}

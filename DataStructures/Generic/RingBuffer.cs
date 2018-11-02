using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// Statically-sized ring buffer container.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T> : ICollection<T>
    {
        private T[] values = null;
        private int availableCount = 0;
        private int headIndex = -1;

        public int Size => values.Length;
        public int Count => availableCount;
        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= availableCount)
                {
                    throw new IndexOutOfRangeException();
                }

                return values[(Size + headIndex - index) % Size];
            }

            set
            {
                if (index < 0 || index >= availableCount)
                {
                    throw new IndexOutOfRangeException();
                }

                values[(Size + headIndex - index) % Size] = value;
            }
        }

        /// <summary>
        /// Returns the head (the most recent) element.
        /// </summary>
        public T Top => this[0];

        /// <summary>
        /// Construct an empty ring buffer supporting bufferSize elements
        /// </summary>
        /// <param name="bufferSize"></param>
        public RingBuffer(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Initialized a RingBuffer with size {bufferSize}.");
            }

            values = new T[bufferSize];
            availableCount = 0;
            headIndex = -1;
        }

        /// <summary>
        /// Copy the list into a new buffer, optionally specify size.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="bufferSize"></param>
        public RingBuffer(ICollection<T> values, int bufferSize = -1)
        {
            if (bufferSize == -1)
            {
                bufferSize = values.Count;
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Attempted to initialize RingBuffer with size {bufferSize}.");
            }

            this.values = new T[bufferSize];

            availableCount = Math.Min(bufferSize, values.Count);
            headIndex = availableCount - 1;

            //Iterate over collection, up to availableCount, and add items
            //We add back to front because newer items go to higher indices in our buffer

            int i = -1;
            using (var e = values.GetEnumerator())
            {
                while (e.MoveNext() && ++i < availableCount)
                {
                    this.values[headIndex - i] = e.Current;
                }
            }
        }

        /// <summary>
        /// Add newValue to the end of the ringbuffer.
        /// Replaces the oldest member if at capacity.
        /// </summary>
        /// <param name="newValue"></param>
        public void Push(T newValue)
        {
            Add(newValue);
        }

        /// <summary>
        /// Add newValue to the end of the ringbuffer.
        /// Replaces the oldest member if at capacity.
        /// </summary>
        /// <param name="newValue"></param>
        public void Add(T newValue)
        {
            availableCount = Math.Min(availableCount + 1, Size);
            headIndex = (headIndex + 1) % Size;
            values[headIndex] = newValue;
        }

        /// <summary>
        /// Clear the current items in the ring buffer.
        /// Doesn't resize or release buffer memory.
        /// Does release item handles.
        /// </summary>
        public void Clear()
        {
            availableCount = 0;
            headIndex = -1;

            for (int i = 0; i < Size; i++)
            {
                values[i] = default(T);
            }
        }

        /// <summary>
        /// Query the list for the argument value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return GetIndex(value) != -1;
        }

        /// <summary>
        /// Get the index of the argument value if it's present.  Otherwise returns -1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetIndex(T value)
        {
            for (int i = 0; i < availableCount; i++)
            {
                if (Comparer<T>.Default.Compare(this[i], value) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the first element matching the argument value, if present, returns whether a value was removed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Remove(T value)
        {
            int index = GetIndex(value);

            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes the item at index.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException">
        /// Throws System.IndexOutOfRangeException if the index exceeds the available count.
        /// </exception>
        public void RemoveAt(int index)
        {
            if (index >= availableCount)
            {
                throw new IndexOutOfRangeException();
            }

            if (index < (availableCount + 1) / 2)
            {
                //If the item we're removing is closer to the front, move items forward
                for (int i = index; i > 0; i--)
                {
                    this[i] = this[i - 1];
                }

                --headIndex;
            }
            else
            {
                for (int i = index; i < availableCount - 1; i++)
                {
                    this[i] = this[i + 1];
                }
            }

            --availableCount;

            if (availableCount == 0)
            {
                headIndex = -1;
            }
        }

        /// <summary>
        /// Removes and returns the item at the head (the newest).
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            T temp = this[0];
            RemoveAt(0);
            return temp;
        }

        /// <summary>
        /// Removes and returns the item at the tail (the oldest).
        /// </summary>
        /// <returns></returns>
        public T PopBack()
        {
            T temp = this[availableCount - 1];
            RemoveAt(availableCount - 1);
            return temp;
        }

        /// <summary>
        /// Returns the number of elements whose value match the argument.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int CountElement(T value)
        {
            int count = 0;

            for (int i = 0; i < availableCount; i++)
            {
                if (Comparer<T>.Default.Compare(this[i], value) == 0)
                {
                    ++count;
                }
            }

            return count;
        }

        /// <summary>
        /// Copy the list to the dest array, using the destIndex as an offset to the destination.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="destIndex"></param>
        public void CopyTo(T[] dest, int destIndex)
        {
            // We use the naive method because we would have to reverse the order of elements anyway
            for (int i = 0; i < Size && i < (destIndex + dest.Length); i++)
            {
                dest[destIndex + i] = this[i];
            }
        }

        /// <summary>
        /// Resize the buffer of this list to support bufferSize elements.
        /// </summary>
        /// <param name="bufferSize"></param>
        public void Resize(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Initialized a RingBuffer with size {bufferSize}.");
            }

            if (bufferSize == Size)
            {
                return;
            }

            int newItemCount = Math.Min(availableCount, bufferSize);
            int newHeadIndex = newItemCount - 1;

            T[] newValues = new T[bufferSize];

            for (int i = 0; i < newItemCount; i++)
            {
                newValues[newHeadIndex - i] = this[i];
            }

            values = newValues;
            headIndex = newHeadIndex;
            availableCount = newItemCount;
        }

        public RingBufferEnum<T> GetRingEnumerator() => 
            new RingBufferEnum<T>(values, availableCount, headIndex);

        public IEnumerator<T> GetEnumerator() => GetRingEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetRingEnumerator();
    }

    /// <summary>
    /// RingBuffer Enumerator class to enable proper list navigation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBufferEnum<T> : IEnumerator<T>
    {
        public T[] values = null;
        public int availableCount = 0;
        public int headIndex = 0;

        private int index = -1;

        public int Size => values.Length;


        public RingBufferEnum(T[] values, int availableCount, int headIndex)
        {
            this.values = values;
            this.availableCount = availableCount;
            this.headIndex = headIndex;
        }

        public bool MoveNext()
        {
            if (index == -1)
            {
                index = headIndex;
                return true;
            }

            //Avoiding Negative mod issues by adding the cycle length before mod
            index = (Size + index - 1) % Size;

            //We have reached the end of the list if the mod distance from our head to our index is
            //equal to our available count, or if we're pointing at the head again
            return (index != headIndex) &&
                ((headIndex - index + Size) % Size < availableCount);
        }

        public void Reset()
        {
            index = -1;
        }

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public T Current
        {
            get
            {
                try
                {
                    return values[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

}
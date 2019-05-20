using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// Statically-sized ring buffer container.
    /// </summary>
    public class RingBuffer<T> : IEnumerable<T>, ICollection<T>
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
        /// Returns the element at the head (the most recent).
        /// </summary>
        public T Head => this[0];

        /// <summary>
        /// Returns the element at the Tail (the oldest).
        /// </summary>
        public T Tail => this[availableCount - 1];

        /// <summary>
        /// Construct an empty ring buffer supporting bufferSize elements
        /// </summary>
        /// <param name="bufferSize">The RingBuffer size</param>
        public RingBuffer(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Initialized a RingBuffer with size {bufferSize}.",
                    paramName: nameof(bufferSize));
            }

            values = new T[bufferSize];
            availableCount = 0;
            headIndex = -1;
        }

        /// <summary>
        /// Copy the list into a new buffer, optionally specify size.
        /// Size defaults to the Count of values
        /// </summary>
        /// <param name="bufferSize">Optional RingBuffer size.  -1 uses the Count of values instead</param>
        public RingBuffer(IEnumerable<T> values, int bufferSize = -1)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (bufferSize == -1)
            {
                bufferSize = values.Count();
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Initialized a RingBuffer with size {bufferSize}.",
                    paramName: nameof(bufferSize));
            }

            this.values = new T[bufferSize];

            availableCount = Math.Min(bufferSize, values.Count());
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
        /// Replaces the oldest element if at capacity.
        /// </summary>
        public void Push(T newValue)
        {
            Add(newValue);
        }

        /// <summary>
        /// Add newValue to the end of the ringbuffer.
        /// Replaces the oldest element if at capacity.
        /// </summary>
        public void Add(T newValue)
        {
            availableCount = Math.Min(availableCount + 1, Size);
            headIndex = (headIndex + 1) % Size;
            values[headIndex] = newValue;
        }

        /// <summary>
        /// Add newValues to the end of the ringbuffer.
        /// Replaces the oldest members if at capacity.
        /// </summary>
        public void AddRange(IEnumerable<T> newValues)
        {
            availableCount = Math.Min(availableCount + newValues.Count(), Size);

            foreach(T newValue in newValues)
            {
                headIndex = (headIndex + 1) % Size;
                values[headIndex] = newValue;
            }
        }

        /// <summary>
        /// Clears the RingBuffer and fills it with optional <paramref name="count"/> default elements.
        /// If <paramref name="count"/> is -1, then the RingBuffer is completely filled.
        /// </summary>
        /// <param name="count">The number of default elements to provide.  If this is -1, the buffer is filled</param>
        public void ZeroOut(int count = -1)
        {
            if (count == -1)
            {
                count = Size;
            }

            if (count < 0 || count > Size)
            {
                throw new ArgumentException($"Called ZeroOut on RingBuffer with invalid count: {count}.",
                    paramName: nameof(count));
            }

            //Clear already sets the values to zero
            Clear();

            availableCount = count;
            headIndex = count - 1;
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
                values[i] = default;
            }
        }

        /// <summary>
        /// Query the list for the argument value.
        /// </summary>
        /// <returns>True if the available range contains value</returns>
        public bool Contains(T value)
        {
            return GetIndex(value) != -1;
        }

        /// <summary>
        /// Get the index of the argument value if it's present.  Otherwise returns -1.
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>Index if the available range contains value, otherwise -1</returns>
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
        /// <param name="value">The value to remove</param>
        /// <returns>Returns success</returns>
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
        /// Removes the element at index.
        /// </summary>
        /// <param name="index">The index of the element to remove</param>
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

                //Clear the copy of the last remaining element
                //We don't want to retain references to dead elements
                this[0] = default;

                //Cyclically decrement headIndex
                headIndex = (Size + headIndex - 1) % Size;
            }
            else
            {
                for (int i = index; i < availableCount - 1; i++)
                {
                    this[i] = this[i + 1];
                }

                //Clear the copy of the last remaining element
                //We don't want to retain references to dead elements
                this[availableCount - 1] = default;
            }

            --availableCount;

            if (availableCount == 0)
            {
                headIndex = -1;
            }
        }

        /// <summary>
        /// Removes and returns the element at the head (the newest).
        /// </summary>
        /// <returns>The element previously at the head</returns>
        public T Pop()
        {
            T temp = this[0];
            RemoveAt(0);
            return temp;
        }

        /// <summary>
        /// Removes and returns the element at the tail (the oldest).
        /// </summary>
        /// <returns>The element previously at the tail</returns>
        public T PopBack()
        {
            T temp = this[availableCount - 1];
            RemoveAt(availableCount - 1);
            return temp;
        }

        /// <summary>
        /// Returns the element at the head (the newest).
        /// </summary>
        /// <returns>The element at the head</returns>
        public T PeekHead() => this[0];

        /// <summary>
        /// Returns the element at the tail (the oldest).
        /// </summary>
        /// <returns>The element at the tail</returns>
        public T PeekTail() => this[availableCount - 1];

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
        /// <param name="bufferSize">The new size</param>
        public void Resize(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Initialized a RingBuffer with size {bufferSize}.",
                    paramName: nameof(bufferSize));
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
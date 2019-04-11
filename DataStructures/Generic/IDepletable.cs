using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// An interface for a collection that tracks and retains underlying values, allowing values to be
    /// popped like a queue or stack, but reset to their undepleted state.
    /// </summary>
    public interface IDepletable<T> : IEnumerable<T>, ICollection<T>
    {
        /// <summary>
        /// Determines whether the bag is automatically replenished when the last item is popped
        /// </summary>
        bool AutoRefill { get; set; }

        /// <summary>
        /// Removes and returns the next value in the IDepletable
        /// </summary>
        T PopNext();

        /// <summary>
        /// Tries to remove the next value in the IDepletable and returns success
        /// </summary>
        bool TryPopNext(out T value);

        /// <summary>
        /// Fills the bag back up.
        /// </summary>
        void Reset();

        /// <summary>
        /// Mark the first instance of value as depleted
        /// </summary>
        bool DepleteValue(T value);

        /// <summary>
        /// Mark all instances of value as depleted
        /// </summary>
        bool DepleteAllValue(T value);

        /// <summary>
        /// Returns a list of available items
        /// </summary>
        IList<T> GetAvailable();

        #region ICollection<T> Extended Methods

        /// <summary>
        /// The total number of underlying valus
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Seaches active and depleted items for value
        /// </summary>
        bool ContainsAnywhere(T value);

        /// <summary>
        /// Copies active and depleted values
        /// </summary>
        void CopyAllTo(T[] array, int arrayIndex);

        #endregion ICollection<T> Extended Methods
    }
}
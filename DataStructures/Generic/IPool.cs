using System;
using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// An interface for a collection that tracks and retains unique underlying values,
    /// allowing values to be checked out and in.
    /// </summary>
    public interface IPool<T> : IEnumerable<T>, ICollection<T>, IPoolRelease<T>
    {
        /// <summary>
        /// Checks Out the next value in the IPool, constructing if necessary
        /// </summary>
        T CheckOut();

        /// <summary>
        /// Checks Out the next value in the IPool meeting the constraint, constructing if necessary
        /// </summary>
        T CheckOut(Func<T, bool> predicate);

        /// <summary>
        /// Tries to CheckOut the next value in the IPool already available, returns success.
        /// </summary>
        bool TryCheckOut(out T value);

        /// <summary>
        /// Tries to CheckOut the next value in the IPool already available and meeting
        /// the constraint, returns success.
        /// </summary>
        bool TryCheckOut(Func<T, bool> predicate, out T value);

        /// <summary>
        /// Add (or re-add) a value to the available list.
        /// </summary>
        void CheckIn(T value);

        /// <summary>
        /// Mark the value as CheckedOut.
        /// This method is somewhat internal and meant to bypass higher-level checks.
        /// </summary>
        void MarkedCheckedOut(T value);

        /// <summary>
        /// Mark the value as CheckedIn.
        /// This method is somewhat internal and meant to bypass higher-level checks.
        /// </summary>
        void MarkedCheckedIn(T value);

        /// <summary>
        /// A enumeration of available items
        /// </summary>
        IEnumerable<T> Available { get; }

        /// <summary>
        /// Returns an enumeration of available items meeting the constraint
        /// </summary>
        IEnumerable<T> GetAvailable(Func<T, bool> predicate);

        /// <summary>
        /// An enumeration of checked-out items
        /// </summary>
        IEnumerable<T> CheckedOut { get; }

        /// <summary>
        /// Returns an enumeration of checked-out items meeting the constraint
        /// </summary>
        IEnumerable<T> GetCheckedOut(Func<T, bool> predicate);
        
        /// <summary>
        /// Does the available pool contain the argument?
        /// </summary>
        bool AvailableContains(T value);

        /// <summary>
        /// Does the checked-out pool contain the argument?
        /// </summary>
        bool CheckedOutContains(T value);


        #region ICollection<T> Extended Methods

        /// <summary>
        /// The total number of underlying valus
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Seaches active and CheckedOut items for value
        /// </summary>
        bool ContainsAnywhere(T value);

        /// <summary>
        /// Copies active and CheckedOut values
        /// </summary>
        void CopyAllTo(T[] array, int arrayIndex);

        #endregion ICollection<T> Extended Methods
    }

    /// <summary>
    /// A convenience interface to safely allow other systems the ability to release pooled objects.
    /// </summary>
    public interface IPoolRelease<T>
    {
        /// <summary>
        /// Release a value back to the pool.
        /// </summary>
        void Release(T value);
    }
}

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace BGC.Parameters
{
    /// <summary>
    /// Display attribute of a tuple-list input field: a List&lt;(T1, T2[, T3[, T4]])&gt; whose rows
    /// have 2 to 4 typed columns, for structured items too small to be property groups, e.g.
    /// (input level, output level) pairs.
    ///
    /// The columns are declared with one class-level <see cref="TupleColumnAttribute"/> each
    /// (IntTupleColumn, DoubleTupleColumn, StringTupleColumn, BoolTupleColumn), giving the column
    /// its name in the saved JSON, its title, its bounds and its initial values. This attribute
    /// holds what applies to the whole list: the title, the allowed row count, and the legacy
    /// string format. See ListFields.md.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TupleListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        /// <summary>
        /// With <see cref="ListFieldDisplayAttribute.legacyDelimiters"/>, upgrades a former delimited
        /// string: rows are split on legacyDelimiters, then each row on these characters.
        /// </summary>
        public readonly char[] legacyColumnDelimiters;

        public TupleListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            char[] legacyDelimiters = null,
            char[] legacyColumnDelimiters = null)
            : base(fieldName, displayTitle, minimumCount, maximumCount, legacyDelimiters)
        {
            this.legacyColumnDelimiters = legacyColumnDelimiters;
        }

        /// <summary>Rows are value tuples; their columns are described by <see cref="TupleListLayout"/>.</summary>
        public override Type ElementType => typeof(ITuple);

        /// <summary>Not available from the attribute alone; use <see cref="TupleListLayout.CreateNewRow"/>.</summary>
        public override object NewItemValue =>
            throw new NotSupportedException("A tuple list's new row comes from its columns: use TupleListLayout.CreateNewRow.");

        /// <summary>Per-column bounds are checked by <see cref="TupleListLayout.IsRowValid"/>.</summary>
        public override bool IsItemValid(object item) => item is ITuple;

        /// <summary>
        /// The initial rows come from the column attributes, so InitializeProperty builds them with
        /// <see cref="TupleListLayout.CreateInitialList"/>.
        /// </summary>
        public override object GetInitialValue() => null;
    }
}

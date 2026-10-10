using System;

namespace BGC.Parameters
{
    /// <summary>
    /// One column of a tuple-list field (see <see cref="TupleListFieldDisplayAttribute"/>).
    /// Declared at class level, like the field display attributes, so a subclass can redeclare a
    /// column to change its title, bounds or initial values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class TupleColumnAttribute : Attribute
    {
        /// <summary>The tuple-list field this column belongs to.</summary>
        public readonly string fieldName;

        /// <summary>Position in the tuple, from 0 (Item1).</summary>
        public readonly int index;

        /// <summary>The column's key in each saved row; must not change once batteries use it.</summary>
        public readonly string columnName;

        public readonly string displayTitle;

        protected TupleColumnAttribute(
            string fieldName,
            int index,
            string columnName,
            string displayTitle)
        {
            this.fieldName = fieldName;
            this.index = index;
            this.columnName = columnName;
            this.displayTitle = displayTitle;
        }

        public abstract Type ElementType { get; }

        /// <summary>The value of this column in a row the user adds, and in a saved row missing the column.</summary>
        public abstract object NewItemValue { get; }

        /// <summary>This column's value in each initial row.</summary>
        public abstract Array InitialValues { get; }

        public virtual string Postfix => "";

        public virtual bool IsItemValid(object item) => true;
    }
}

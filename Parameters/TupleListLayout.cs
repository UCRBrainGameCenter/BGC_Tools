using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BGC.Parameters
{
    /// <summary>
    /// The resolved shape of a tuple-list field on a property group: its row type, its
    /// <see cref="TupleListFieldDisplayAttribute"/> and its columns in order. Rows are boxed
    /// value tuples, read through <see cref="ITuple"/> and built with <see cref="CreateRow"/>.
    /// </summary>
    public sealed class TupleListLayout
    {
        public const int MinimumColumns = 2;
        public const int MaximumColumns = 4;

        public Type RowType { get; }
        public TupleListFieldDisplayAttribute Attribute { get; }
        public IReadOnlyList<TupleColumnAttribute> Columns { get; }

        private TupleListLayout(
            Type rowType,
            TupleListFieldDisplayAttribute attribute,
            IReadOnlyList<TupleColumnAttribute> columns)
        {
            RowType = rowType;
            Attribute = attribute;
            Columns = columns;
        }

        /// <summary>
        /// The row type if this is a List of a 2- to 4-item value tuple whose items are all
        /// int, double, string or bool; otherwise null.
        /// </summary>
        public static Type GetTupleRowType(PropertyInfo property)
        {
            Type rowType = property.GetListElementType();
            if (rowType == null || !IsValueTuple(rowType))
            {
                return null;
            }

            Type[] itemTypes = rowType.GetGenericArguments();
            if (itemTypes.Length < MinimumColumns || itemTypes.Length > MaximumColumns ||
                !itemTypes.All(ListFieldSerialization.IsSupportedElementType))
            {
                return null;
            }

            return rowType;
        }

        public static bool IsValueTuple(Type type) =>
            type.IsGenericType && type.FullName != null && type.FullName.StartsWith("System.ValueTuple`");

        /// <summary>
        /// The layout of a tuple-list field, or null if the property is not a tuple list or its
        /// attributes are missing or inconsistent (the declaration check reports why).
        /// </summary>
        public static TupleListLayout Get(Type propertyGroupType, PropertyInfo property)
        {
            Type rowType = GetTupleRowType(property);
            if (rowType == null)
            {
                return null;
            }

            string fieldName = property.GetInputFieldName();

            TupleListFieldDisplayAttribute attribute = propertyGroupType
                .GetCustomAttributes_Deep<FieldDisplayAttribute>()
                .FirstOrDefault(x => x.fieldName == fieldName) as TupleListFieldDisplayAttribute;

            if (attribute == null)
            {
                return null;
            }

            TupleColumnAttribute[] columns = GetColumns(propertyGroupType, fieldName);
            Type[] itemTypes = rowType.GetGenericArguments();

            if (columns.Length != itemTypes.Length)
            {
                return null;
            }

            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].index != i || columns[i].ElementType != itemTypes[i])
                {
                    return null;
                }
            }

            return new TupleListLayout(rowType, attribute, columns);
        }

        public static TupleListLayout Get(IPropertyGroup propertyGroup, PropertyInfo property) =>
            Get(propertyGroup.GetType(), property);

        /// <summary>
        /// The column attributes for a field, one per index; where a subclass redeclares an index,
        /// the subclass's attribute wins (as for field display attributes).
        /// </summary>
        public static TupleColumnAttribute[] GetColumns(Type propertyGroupType, string fieldName) =>
            propertyGroupType
                .GetCustomAttributes_Deep<TupleColumnAttribute>()
                .Where(x => x.fieldName == fieldName)
                .GroupBy(x => x.index)
                .Select(x => x.First())
                .OrderBy(x => x.index)
                .ToArray();

        public object CreateRow(IReadOnlyList<object> values) =>
            Activator.CreateInstance(RowType, values.ToArray());

        public object CreateNewRow() =>
            CreateRow(Columns.Select(x => x.NewItemValue).ToArray());

        public object GetItem(object row, int column) => ((ITuple)row)[column];

        /// <summary>A copy of the row with one column replaced.</summary>
        public object WithItem(object row, int column, object value)
        {
            object[] values = Enumerable.Range(0, Columns.Count).Select(i => GetItem(row, i)).ToArray();
            values[column] = value;
            return CreateRow(values);
        }

        public bool IsRowValid(object row) =>
            row is ITuple tuple &&
            tuple.Length == Columns.Count &&
            Enumerable.Range(0, Columns.Count).All(i => Columns[i].IsItemValid(tuple[i]));

        public bool IsListValid(IList list) =>
            list != null &&
            Attribute.IsCountValid(list.Count) &&
            list.Cast<object>().All(IsRowValid);

        /// <summary>
        /// A fresh list of the initial rows, built from the columns' initial values. Columns with
        /// fewer initial values than the longest one use their new-item value for the rest.
        /// </summary>
        public IList CreateInitialList()
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(RowType));
            int rowCount = Columns.Max(x => x.InitialValues.Length);

            for (int row = 0; row < rowCount; row++)
            {
                list.Add(CreateRow(Columns
                    .Select(x => row < x.InitialValues.Length ? x.InitialValues.GetValue(row) : x.NewItemValue)
                    .ToArray()));
            }

            return list;
        }
    }
}

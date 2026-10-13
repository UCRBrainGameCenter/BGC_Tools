using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoolTupleColumnAttribute : TupleColumnAttribute
    {
        public readonly bool[] initial;
        public readonly bool newItem;

        public BoolTupleColumnAttribute(
            string fieldName,
            int index,
            string columnName,
            string displayTitle,
            bool[] initial = null,
            bool newItem = false)
            : base(fieldName, index, columnName, displayTitle)
        {
            this.initial = initial ?? Array.Empty<bool>();
            this.newItem = newItem;
        }

        public override Type ElementType => typeof(bool);

        public override object NewItemValue => newItem;

        public override Array InitialValues => initial;

        public override bool IsItemValid(object item) => item is bool;
    }
}

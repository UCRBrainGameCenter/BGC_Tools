using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringTupleColumnAttribute : TupleColumnAttribute
    {
        public readonly string[] initial;
        public readonly string newItem;

        public StringTupleColumnAttribute(
            string fieldName,
            int index,
            string columnName,
            string displayTitle,
            string[] initial = null,
            string newItem = "")
            : base(fieldName, index, columnName, displayTitle)
        {
            this.initial = initial ?? Array.Empty<string>();
            this.newItem = newItem ?? "";
        }

        public override Type ElementType => typeof(string);

        public override object NewItemValue => newItem;

        public override Array InitialValues => initial;

        public override bool IsItemValid(object item) => item is string;
    }
}

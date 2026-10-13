using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntTupleColumnAttribute : TupleColumnAttribute
    {
        public readonly int[] initial;
        public readonly int minimum;
        public readonly int maximum;
        public readonly int newItem;

        public readonly string postfix;

        public IntTupleColumnAttribute(
            string fieldName,
            int index,
            string columnName,
            string displayTitle,
            int[] initial = null,
            int minimum = int.MinValue,
            int maximum = int.MaxValue,
            string postfix = "",
            int newItem = 0)
            : base(fieldName, index, columnName, displayTitle)
        {
            this.initial = initial ?? Array.Empty<int>();
            this.minimum = minimum;
            this.maximum = maximum;
            this.newItem = newItem;

            this.postfix = postfix;
        }

        public override Type ElementType => typeof(int);

        public override object NewItemValue => Math.Min(Math.Max(newItem, minimum), maximum);

        public override Array InitialValues => initial;

        public override string Postfix => postfix;

        public override bool IsItemValid(object item) =>
            item is int value && value >= minimum && value <= maximum;
    }
}

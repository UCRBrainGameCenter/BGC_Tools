using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DoubleTupleColumnAttribute : TupleColumnAttribute
    {
        public readonly double[] initial;
        public readonly double minimum;
        public readonly double maximum;
        public readonly double newItem;

        public readonly string postfix;

        public DoubleTupleColumnAttribute(
            string fieldName,
            int index,
            string columnName,
            string displayTitle,
            double[] initial = null,
            double minimum = double.MinValue,
            double maximum = double.MaxValue,
            string postfix = "",
            double newItem = 0.0)
            : base(fieldName, index, columnName, displayTitle)
        {
            this.initial = initial ?? Array.Empty<double>();
            this.minimum = minimum;
            this.maximum = maximum;
            this.newItem = newItem;

            this.postfix = postfix;
        }

        public override Type ElementType => typeof(double);

        public override object NewItemValue => Math.Min(Math.Max(newItem, minimum), maximum);

        public override Array InitialValues => initial;

        public override string Postfix => postfix;

        public override bool IsItemValid(object item) =>
            item is double value && value >= minimum && value <= maximum;
    }
}

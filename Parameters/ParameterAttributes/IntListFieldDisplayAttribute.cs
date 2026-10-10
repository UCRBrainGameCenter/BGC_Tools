using System;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly int[] initial;
        public readonly int minimum;
        public readonly int maximum;
        public readonly int newItem;

        public readonly string postfix;

        public IntListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int[] initial = null,
            int minimum = int.MinValue,
            int maximum = int.MaxValue,
            string postfix = "",
            int newItem = 0,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            char[] legacyDelimiters = null)
            : base(fieldName, displayTitle, minimumCount, maximumCount, legacyDelimiters)
        {
            this.initial = initial ?? Array.Empty<int>();
            this.minimum = minimum;
            this.maximum = maximum;
            this.newItem = newItem;

            this.postfix = postfix;
        }

        public override Type ElementType => typeof(int);

        public override object NewItemValue => Math.Min(Math.Max(newItem, minimum), maximum);

        public override bool IsItemValid(object item) =>
            item is int value && value >= minimum && value <= maximum;

        // A fresh list every call, so property groups never share an instance.
        public override object GetInitialValue() => new List<int>(initial);
    }
}

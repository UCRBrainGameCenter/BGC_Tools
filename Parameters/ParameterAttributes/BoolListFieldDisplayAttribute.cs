using System;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoolListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly bool[] initial;
        public readonly bool newItem;

        public BoolListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            bool[] initial = null,
            bool newItem = false,
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            char[] legacyDelimiters = null)
            : base(fieldName, displayTitle, minimumCount, maximumCount, legacyDelimiters)
        {
            this.initial = initial ?? Array.Empty<bool>();
            this.newItem = newItem;
        }

        public override Type ElementType => typeof(bool);

        public override object NewItemValue => newItem;

        public override bool IsItemValid(object item) => item is bool;

        // A fresh list every call, so property groups never share an instance.
        public override object GetInitialValue() => new List<bool>(initial);
    }
}

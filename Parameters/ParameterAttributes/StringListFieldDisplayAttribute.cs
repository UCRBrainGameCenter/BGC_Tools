using System;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringListFieldDisplayAttribute : ListFieldDisplayAttribute
    {
        public readonly string[] initial;
        public readonly string newItem;

        public StringListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            string[] initial = null,
            string newItem = "",
            int minimumCount = 0,
            int maximumCount = int.MaxValue,
            char[] legacyDelimiters = null)
            : base(fieldName, displayTitle, minimumCount, maximumCount, legacyDelimiters)
        {
            this.initial = initial ?? Array.Empty<string>();
            this.newItem = newItem ?? "";
        }

        public override Type ElementType => typeof(string);

        public override object NewItemValue => newItem;

        public override bool IsItemValid(object item) => item is string;

        // A fresh list every call, so property groups never share an instance.
        public override object GetInitialValue() => new List<string>(initial);
    }
}

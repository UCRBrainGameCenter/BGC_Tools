using System;
using System.Collections;

namespace BGC.Parameters
{
    /// <summary>
    /// Base for the display attributes of List&lt;T&gt; input fields.
    /// Declared at class level and paired with a [DisplayInputField] property, like the scalar
    /// FieldDisplay attributes. Serialization is driven by the property type (see
    /// <see cref="ListFieldSerialization"/>); this attribute supplies the initial items, the
    /// allowed item count, per-item bounds, and the editor behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ListFieldDisplayAttribute : FieldDisplayAttribute
    {
        public readonly int minimumCount;
        public readonly int maximumCount;

        /// <summary>
        /// When set, a serialized string in place of the list is split on these characters and
        /// each piece parsed as an item. This upgrades fields that used to be a delimited string.
        /// Including '\n' also splits on "\r\n" and '\r'. Null means strings are not split.
        /// </summary>
        public readonly char[] legacyDelimiters;

        protected ListFieldDisplayAttribute(
            string fieldName,
            string displayTitle,
            int minimumCount,
            int maximumCount,
            char[] legacyDelimiters)
            : base(fieldName, displayTitle)
        {
            this.minimumCount = Math.Max(0, minimumCount);
            this.maximumCount = Math.Max(this.minimumCount, maximumCount);
            this.legacyDelimiters = legacyDelimiters;
        }

        /// <summary>The item type. The property must be a List of exactly this type.</summary>
        public abstract Type ElementType { get; }

        /// <summary>The item appended when a user adds one in the editor.</summary>
        public abstract object NewItemValue { get; }

        /// <summary>Whether a single item is within this field's per-item bounds.</summary>
        public virtual bool IsItemValid(object item) => true;

        public bool IsCountValid(int count) => count >= minimumCount && count <= maximumCount;

        public bool IsListValid(IList list)
        {
            if (list == null || !IsCountValid(list.Count))
            {
                return false;
            }

            foreach (object item in list)
            {
                if (!IsItemValid(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

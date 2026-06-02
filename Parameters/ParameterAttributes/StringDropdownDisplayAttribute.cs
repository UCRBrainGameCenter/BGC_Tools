using System;

namespace BGC.Parameters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StringDropdownDisplayAttribute : FieldDisplayAttribute
    {
        public readonly string choiceListMethodName;
        public readonly string initialValue;
        public readonly bool retainMissingValues;
        public readonly bool forceRefreshOnChange;

        /// <summary>
        /// Optional — name of an instance method on the owning IPropertyGroup that takes no
        /// arguments and returns the IPropertyGroup that the current dropdown value refers
        /// to (or null if the value is unset / unresolvable). When set, the column-stack
        /// FormColumn renders a delve chevron next to the dropdown for navigating into the
        /// referenced group (e.g. SubBatteryStageInfo's "GetBattery" method resolves the
        /// chosen BatteryName to the actual BatteryInfo).
        /// </summary>
        public readonly string resolveMethodName;

        /// <summary>
        /// Optional — name of an instance method on the owning IPropertyGroup with signature
        /// <c>void Method(Action&lt;string&gt; onCreated)</c>. When set, the column-stack
        /// FormColumn appends a synthetic <see cref="createLabel"/> entry to the dropdown.
        /// Selecting it invokes the method, which is expected to spawn a UI flow (e.g. a name
        /// prompt + creation), then call the callback with the new value (or null on cancel).
        /// The reflector wires that callback to set the field + refresh.
        /// </summary>
        public readonly string createMethodName;

        /// <summary>
        /// Display text for the synthetic create entry — defaults to "+ New…" when null.
        /// </summary>
        public readonly string createLabel;

        /// <summary>
        /// Optional — when set, the column-stack FormColumn renders this label as a synthetic
        /// first entry in the dropdown. An empty / unmatched current value lands on it instead
        /// of misleadingly highlighting the first real choice; selecting it writes empty back.
        /// Typical use: "(none)" for reference fields where a freshly-added stage hasn't yet
        /// picked a target.
        /// </summary>
        public readonly string placeholderLabel;

        public StringDropdownDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initialValue,
            bool retainMissingValues,
            string choiceListMethodName,
            bool forceRefreshOnChange = false,
            string resolveMethodName = null,
            string createMethodName = null,
            string createLabel = null,
            string placeholderLabel = null)
            : base(fieldName, displayTitle)
        {
            this.initialValue = initialValue;
            this.retainMissingValues = retainMissingValues;
            this.choiceListMethodName = choiceListMethodName;
            this.forceRefreshOnChange = forceRefreshOnChange;
            this.resolveMethodName = resolveMethodName;
            this.createMethodName = createMethodName;
            this.createLabel = createLabel;
            this.placeholderLabel = placeholderLabel;
        }

        public override object GetInitialValue() => initialValue;
    }
}

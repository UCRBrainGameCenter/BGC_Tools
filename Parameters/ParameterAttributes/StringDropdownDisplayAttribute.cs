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

        public StringDropdownDisplayAttribute(
            string fieldName,
            string displayTitle,
            string initialValue,
            bool retainMissingValues,
            string choiceListMethodName,
            bool forceRefreshOnChange = false,
            string resolveMethodName = null)
            : base(fieldName, displayTitle)
        {
            this.initialValue = initialValue;
            this.retainMissingValues = retainMissingValues;
            this.choiceListMethodName = choiceListMethodName;
            this.forceRefreshOnChange = forceRefreshOnChange;
            this.resolveMethodName = resolveMethodName;
        }

        public override object GetInitialValue() => initialValue;
    }
}

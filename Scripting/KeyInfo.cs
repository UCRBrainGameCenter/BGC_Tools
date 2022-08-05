using System;

namespace BGC.Scripting
{
    public readonly struct KeyInfo
    {
        public readonly Type valueType;
        public readonly string key;

        public KeyInfo(
            Type valueType,
            string key)
        {
            this.valueType = valueType;
            this.key = key;
        }
    }
}
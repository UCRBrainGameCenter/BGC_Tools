using System;

namespace BGC.Parameters
{
    public readonly struct ValueNamePair
    {
        public readonly int value;
        public readonly string name;

        public ValueNamePair(int value, string name)
        {
            this.value = value;
            this.name = name;
        }
    }
}

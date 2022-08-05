using System;

namespace BGC.Scripting
{
    public static class TokenExtensions
    {
        public static object GetDefaultValue(this Type valueType)
        {
            if (valueType.IsValueType)
            {
                return Activator.CreateInstance(valueType);
            }

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Parameters
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<T> GetCustomAttributes_Deep<T>(this Type type) => type
            .GetCustomAttributes(typeof(T), true)
            .Union(type.GetInterfaces()
            .SelectMany(interfaceType => interfaceType.GetCustomAttributes(typeof(T), true)))
            .Distinct()
            .Cast<T>();
    }
}

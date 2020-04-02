using System;

namespace BGC.Extensions
{
    public static class StringExtensions
    {
        public static string Encode(this string str) => Uri.EscapeDataString(str);
        public static string Encode(this IConvertible convertible) => Uri.EscapeDataString(convertible.ToString());

        public static string Decode(this string str) => Uri.UnescapeDataString(str);
    }
}

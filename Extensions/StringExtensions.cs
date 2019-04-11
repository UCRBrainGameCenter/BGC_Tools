namespace BGC.Extensions
{
    public static class StringExtensions
    {
        public static string Encode(this string str)
        {
            return System.Uri.EscapeDataString(str);
        }

        public static string Decode(this string str)
        {
            return System.Uri.UnescapeDataString(str);
        }
    }
}
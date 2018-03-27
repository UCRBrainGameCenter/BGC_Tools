using System.IO;

namespace BGC.IO
{
    public static class Extensions
    {
        public const string JSON = ".json";
        public const string CSV  = ".csv";
        public const string XML  = ".xml";

        /// <summary>
        /// Add json extension to string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddJsonExtension(string str)
        {
            return addExtension(str, JSON);
        }

        /// <summary>
        /// Add csv extension to string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddCsvExtension(string str)
        {
            return addExtension(str, CSV);
        }

        /// <summary>
        /// Add xml extension to string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddXmlExtension(string str)
        {
            return addExtension(str, XML);
        }

        /// <summary>
        /// Return true if the string ends with json extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasJsonExtension(string str)
        {
            return str.EndsWith(JSON);
        }

        /// <summary>
        /// Return true if the string ends with csv extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasCsvExtension(string str)
        {
            return str.EndsWith(CSV);
        }

        /// <summary>
        /// Return true if the string ends with xml extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasXmlExtension(string str)
        {
            return str.EndsWith(XML);
        }

        private static string addExtension(string str, string extension)
        {
            return str + extension;
        }
    }
}
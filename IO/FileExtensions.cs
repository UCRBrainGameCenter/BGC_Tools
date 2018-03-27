namespace BGC.IO
{
    public static class FileExtensions
    {
        public const string JSON = ".json";
        public const string CSV  = ".csv";
        public const string XML  = ".xml";
        public const string BGC  = ".bgc";

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
        /// Add bgc extension to string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddBgcExtension(string str)
        {
            return addExtension(str, BGC);
        }

        /// <summary>
        /// Return true if the string ends with the json extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasJsonExtension(string str)
        {
            return str.EndsWith(JSON);
        }

        /// <summary>
        /// Return true if the string ends with the csv extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasCsvExtension(string str)
        {
            return str.EndsWith(CSV);
        }

        /// <summary>
        /// Return true if the string ends with the xml extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasXmlExtension(string str)
        {
            return str.EndsWith(XML);
        }

        /// <summary>
        /// Return true if the string ends with the bgc extension
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasBgcExtension(string str)
        {
            return str.EndsWith(BGC);
        }

        private static string addExtension(string str, string extension)
        {
            return str + extension;
        }
    }
}
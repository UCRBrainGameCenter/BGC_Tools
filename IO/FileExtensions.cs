namespace BGC.IO
{
    public static class FileExtensions
    {
        public const string JSON = ".json";
        public const string CSV = ".csv";
        public const string XML = ".xml";
        public const string BGC = ".bgc";

        /// <summary>
        /// Add json extension to string
        /// </summary>
        public static string AddJsonExtension(string str) => $"{str}{JSON}";

        /// <summary>
        /// Add csv extension to string
        /// </summary>
        public static string AddCsvExtension(string str) => $"{str}{CSV}";

        /// <summary>
        /// Add xml extension to string
        /// </summary>
        public static string AddXmlExtension(string str) => $"{str}{XML}";

        /// <summary>
        /// Add bgc extension to string
        /// </summary>
        public static string AddBgcExtension(string str) => $"{str}{BGC}";

        /// <summary>
        /// Return true if the string ends with the json extension
        /// </summary>
        public static bool HasJsonExtension(string str) => str.EndsWith(JSON);

        /// <summary>
        /// Return true if the string ends with the csv extension
        /// </summary>
        public static bool HasCsvExtension(string str) => str.EndsWith(CSV);

        /// <summary>
        /// Return true if the string ends with the xml extension
        /// </summary>
        public static bool HasXmlExtension(string str) => str.EndsWith(XML);

        /// <summary>
        /// Return true if the string ends with the bgc extension
        /// </summary>
        public static bool HasBgcExtension(string str) => str.EndsWith(BGC);
    }
}
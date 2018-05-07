using UnityEngine;
using System.IO;

namespace BGC.IO
{
    public class LogManagement
    {
        public const string LogDirectory = "Logs";

        /// <summary>
        /// Get path for dat directory root
        /// </summary>
        /// <returns></returns>
        public static string GetDataRootDir()
        {
            string path = Application.persistentDataPath;

            if (Application.platform != RuntimePlatform.IPhonePlayer &&
                Application.platform != RuntimePlatform.Android)
            {
                // Use the DataPath on Development machines
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
            }

            return path;
        }

        /// <summary>
        /// Get the path for the log data directory
        /// </summary>
        /// <returns></returns>
        public static string PathForLogDirectory()
        {
            return Path.Combine(GetDataRootDir(), LogDirectory);
        }

        /// <summary>
        /// Get user directory
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string GetUserDirectory(string userName)
        {
            return Path.Combine(PathForLogDirectory(), userName);
        }
    }
}
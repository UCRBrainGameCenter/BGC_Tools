using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BGC.IO
{
    public class DataManagement
    {
        /// <summary>
        /// Get a list of all data files in sub directory of os data directory
        /// </summary>
        /// <param name="dataSubDir"></param>
        /// <returns></returns>
        public static List<string> GetDataFiles(string dataSubDir)
        {
            string path = PathForDataDirectory(dataSubDir);

            return new List<string>(Directory.GetFiles(path));
        }

        /// <summary>
        /// Get path for dat directory root
        /// </summary>
        /// <returns></returns>
        private static string GetDataRootDir()
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
        /// Returns the full path to the <paramref name="dataSubdir"/> directory.
        /// <param name="dataSubdir"></param>
        /// <returns></returns>
        public static string PathForDataDirectory(string dataSubdir)
        {
            string path = Path.Combine(GetDataRootDir(), dataSubdir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
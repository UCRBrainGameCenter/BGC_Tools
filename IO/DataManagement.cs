using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BGC.IO
{
    public class DataManagement
    {
        private static string rootDirectory = null;

        /// <summary>
        /// Root directory for all users data
        /// </summary>
        public static string RootDirectory
        {
            get
            {
                if (rootDirectory == null)
                {
                    rootDirectory = Application.persistentDataPath;

                    if (Application.platform != RuntimePlatform.IPhonePlayer &&
                        Application.platform != RuntimePlatform.Android)
                    {
                        rootDirectory = Application.dataPath;
                        rootDirectory = rootDirectory.Substring(0, rootDirectory.LastIndexOf('/'));
                    }
                }

                return rootDirectory;
            }
        }

        /// <summary>
        /// Get a list of all data files in sub directory of os data directory
        /// </summary>
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetDataFiles(string dataDirectory) =>
            Directory.GetFiles(PathForDataDirectory(dataDirectory));

        public static string PathForDataFile(string dataDirectory, string fileName) =>
            Path.Combine(PathForDataDirectory(dataDirectory), fileName);

        /// <summary>
        /// Returns the full path to the <paramref name="dataDirectory"/> directory.
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static string PathForDataDirectory(string dataDirectory)
        {
            string path = Path.Combine(RootDirectory, dataDirectory);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Returns the full path to the <paramref name="dataDirectory"/> directory.
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static string PathForDataSubDirectory(params string[] dataDirectories)
        {
            string[] paths = new string[dataDirectories.Length + 1];
            paths[0] = RootDirectory;
            System.Array.Copy(
                sourceArray: dataDirectories,
                sourceIndex: 0,
                destinationArray: paths,
                destinationIndex: 1,
                length: dataDirectories.Length);

            string path = Path.Combine(paths);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            
            return path;
        }
    }
}
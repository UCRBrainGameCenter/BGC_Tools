using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BGC.IO
{
    public class DataManagement
    {
        private static string rootDirectory = null;

        /// <summary> Root directory for all users data </summary>
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

        /// <summary> Get a list of all data files in sub directory of os data directory </summary>
        public static IEnumerable<string> GetDataFiles(string dataDirectory) =>
            Directory.GetFiles(PathForDataDirectory(dataDirectory));

        /// <summary> Returns the full path for specified datafile in a data directory </summary>
        public static string PathForDataFile(string dataDirectory, string fileName) =>
            Path.Combine(PathForDataDirectory(dataDirectory), fileName);

        /// <summary> Returns the full path to the <paramref name="dataDirectory"/> directory. </summary>
        public static string PathForDataDirectory(string dataDirectory)
        {
            string path = Path.Combine(RootDirectory, dataDirectory);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static bool DataDirectoryExists(string dataDirectory) =>
            Directory.Exists(Path.Combine(RootDirectory, dataDirectory));

        /// <summary> Returns the full path to the <paramref name="dataDirectories"/> directory. </summary>
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

        /// <summary>
        /// Returns an available filepath.
        /// Appends " (#)" to the filename, incrementing # until it is available, 
        /// starting with any modifier present in the filepath.
        /// </summary>
        /// <returns>An available filepath</returns>
        [Obsolete("Use FilePath.NextAvailableFilePath instead")]
        public static string NextAvailableFilePath(string filepath) =>
            FilePath.NextAvailableFilePath(filepath);
    }
}
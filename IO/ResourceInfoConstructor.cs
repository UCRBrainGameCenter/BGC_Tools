using System.Collections.Generic;
using UnityEngine.Assertions;
using BGC.Utility;
using System.IO;

namespace BGC.IO
{
    public static class ResourceInfoConstructor
    {
        public const string JsonFileStoragePath = "Assets/Resources/resource_files.json";
        public const string ResourceInfoFile = "resource_files";
        public const string MetaExtension = ".meta";
        public const char CharPathSeparator = '/';
        public const string PathSeparator = "/";

        public static readonly string ResourcePath = Path.Combine("Assets", "Resources");

        #if UNITY_EDITOR
        /// <summary>
        /// Create resource info json file that ResourceInfo.cs uses
        /// </summary>
        public static void ConstructResourceInfoFile()
        {
            // creatre and write json file
            File.WriteAllText(
                JsonFileStoragePath,
                GetAllFilesRecursive(ResourcePath, "").ToJsonArray().ToString());

            // refersh unity editor so it shows up immediately 
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// Gets all nested files within a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetAllFilesRecursive(string path, string resourcePath)
        {
            Assert.IsFalse(string.IsNullOrEmpty(path));
            List<string> filePaths = new List<string>();

            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            DirectoryInfo[] directories = info.GetDirectories();

            string fileName;
            string fileExtension;
            int length = files.Length;
            for (int i = 0; i < length; ++i)
            {
                FileInfo file = files[i];
                fileExtension = file.Extension;

                if (fileExtension != MetaExtension)
                {
                    fileName = file.Name;
                    filePaths.Add(ResourceUtility.Combine(
                        resourcePath, 
                        fileName.Substring(0, fileName.Length - fileExtension.Length)));
                }
            }

            int innerLength;
            List<string> newFilePaths;
            length = directories.Length;
            for (int i = 0; i < length; ++i)
            {
                newFilePaths = GetAllFilesRecursive(
                    Path.Combine(path, directories[i].Name),
                    ResourceUtility.Combine(resourcePath, directories[i].Name));

                innerLength = newFilePaths.Count;
                for (int j = 0; j < innerLength; ++j)
                {
                    filePaths.Add(newFilePaths[j]);
                }
            }

            return filePaths;
        }
        #endif
    }
}
using System.Collections.Generic;
using LightJson.Serialization;
using UnityEngine.Assertions;
using UnityEngine;
using LightJson;
using System;

// @todo: change this, it doesn't belong in bgc io
namespace BGC.IO
{
    public static class ResourceInfo
    {
        private static List<string> resources;
        private static int length;

        public static List<string> ListFiles(string path)
        {
            InitializeResourceInfo();
            List<string> paths = new List<string>();

            if (path == null)
            {
                return null;
            }

            int directoryCount = path.Split(ResourceInfoConstructor.CharPathSeparator).Length;
            string filePath;
            int resourceDirectoryCount;

            for (int i = 0; i < length; ++i)
            {
                filePath = resources[i];

                if (filePath.Contains(path))
                {
                    resourceDirectoryCount = filePath.Split(ResourceInfoConstructor.CharPathSeparator).Length;
                    if (directoryCount == resourceDirectoryCount)
                    {
                        paths.Add(filePath);
                    }
                }
            }

            return paths;
        }

        public static bool Exists(string path)
        {
            InitializeResourceInfo();
            bool exists = false;
            if (string.IsNullOrEmpty(path) == false)
            {
                for (int i = 0; i < length; ++i)
                {
                    if (resources[i].Equals(path, StringComparison.Ordinal))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }

        private static void InitializeResourceInfo()
        {
            if (resources == null)
            {
                #if UNITY_EDITOR
                if (Resources.Load<TextAsset>(ResourceInfoConstructor.JsonFileStoragePath) == null)
                {
                    ResourceInfoConstructor.ConstructResourceInfoFile();
                }
                #endif

                resources = new List<string>();

                TextAsset text = Resources.Load<TextAsset>(ResourceInfoConstructor.ResourceInfoFile);
                Assert.IsNotNull(text);

                JsonArray json = JsonReader.Parse(text.text).AsJsonArray;
                Assert.IsNotNull(json);

                length = json.Count;
                for (int i = 0; i < length; ++i)
                {
                    resources.Add(json[i].AsString);
                }
            }
        }
    }
}
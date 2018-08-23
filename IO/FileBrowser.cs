using LightJson;
using System.IO;
using BGC.Extensions;
using System;
using BGC.Utility;
using UnityEngine.Assertions;

public static class FileBrowser
{
    private class Keys
    {
        public const string files = "Files";
        public const string dirs = "Directories";
    }

    private const string JsonFileStoragePath = "Assets/Resources/ResourceFiles.json";

#if UNITY_EDITOR
    public static void CreateJsonFile(string path)
    {
        JsonObject json = GetAllFilesRecursive(path);
        File.WriteAllText(JsonFileStoragePath, json.ToString());
    }
#endif

    /// <summary>
    /// Gets all nested files within a path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static JsonObject GetAllFilesRecursive(string path)
    {
        JsonObject json = GetAllFiles(path);
        JsonArray contents = new JsonArray();

        JsonArray files = json.TryGetArray(Keys.files);
        int length = files.Count;
        for(int i = 0; i < length; ++i)
        {
            contents.Add(files[i]);
        }

        JsonArray dirs = json.TryGetArray(Keys.dirs);
        length = dirs.Count;
        for(int i = 0; i < length; ++i)
        {
            contents.Add(GetAllFilesRecursive(dirs[i]));
        }

        json = new JsonObject();
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        json.Add(dirInfo.Name, contents);

        return json;
    }

    /// <summary>
    /// Returns a JsonObject with path of all Files/Dirs in a path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static JsonObject GetAllFiles(string path)
    {
        JsonObject json = new JsonObject();
        DirectoryInfo dir = new DirectoryInfo(path);

        DirectoryInfo[] dirInfo = dir.GetDirectories();
        JsonArray dirs = new JsonArray();

        int length = dirInfo.Length;
        for(int i = 0; i < length; ++i)
        {
            dirs.Add(dirInfo[i].FullName);
        }

        json.Add(Keys.dirs, dirs);
      
        FileInfo[] fileInfo = Array.FindAll(dir.GetFiles("*.*"), x => x.Extension.Equals(".meta") == false);
        JsonArray files = new JsonArray();

        length = fileInfo.Length;
        for(int i = 0; i < length; ++i)
        {
            string filepath = fileInfo[i].FullName;
            bool result = ResourceUtility.ConvertToValidResourcePath(ref filepath);
            Assert.IsTrue(result);

            files.Add(filepath);
        }

        json.Add(Keys.files, files);

        return json;
    }
}

using System;
using System.IO;
using UnityEditor;

public static class BGCBar
{
    public const string buildVersionPath = "Assets/Resources/BuildVersion.txt";
    public const string fileName = "BuildVersion";

    [MenuItem("BGC/Update Build Number")]
    static void UpdateBuildNumber()
    {
        string buildNumber = DateTime.Today.ToString("yy.MM.dd");
        PlayerSettings.iOS.buildNumber = buildNumber;
        File.WriteAllText(buildVersionPath, buildNumber);
    }
}

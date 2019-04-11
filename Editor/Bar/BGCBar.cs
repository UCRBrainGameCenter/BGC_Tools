using System;
using UnityEditor;

public static class BGCBar
{
    [MenuItem("BGC/Update Build Number")]
    static void UpdateBuildNumber()
    {
        PlayerSettings.iOS.buildNumber = DateTime.Today.ToString("yy.MM.dd");
    }
}

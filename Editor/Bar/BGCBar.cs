using UnityEditor;
using System;

public static class BGCBar
{
    private const string LogsToServerInEditorEnabled = "EDITOR_SERVER_ENABLED";

    #region Build Settings
    [MenuItem("bgc/server/Push Logs To Server In Editor")]
    private static void SetPushToServerInEditor()
    {
        BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        string settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
        if (settings.Contains(LogsToServerInEditorEnabled) == false)
        {
            if (settings == "" || settings.Equals(";", StringComparison.Ordinal))
            {
                settings = $"{LogsToServerInEditorEnabled};";
            }
            else if(settings.EndsWith(";"))
            {
                settings = $"{settings}{LogsToServerInEditorEnabled}";
            }
            else
            {
                settings = $"{settings};{LogsToServerInEditorEnabled};";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, settings);
        }
    }

    [MenuItem("bgc/server/Do Not Push Logs To Server in Editor")]
    private static void SetNotToPushToServerInEditor()
    {
        BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        string settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
        if (settings.Contains(LogsToServerInEditorEnabled))
        {
            settings = settings.Replace(LogsToServerInEditorEnabled, "");
            settings = settings.Replace(";;", ";");

            if (settings.Equals(";", StringComparison.Ordinal))
            {
                settings = "";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, settings);
        }
    }

    [MenuItem("bgc/Update Build Number")]
    static void UpdateBuildNumber()
    {
        PlayerSettings.iOS.buildNumber = DateTime.Today.ToString("yy.MM.dd");
    }
    #endregion
}
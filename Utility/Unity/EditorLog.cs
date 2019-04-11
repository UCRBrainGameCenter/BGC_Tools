using UnityEngine;

namespace BGC.Utility.Unity
{
    public static class EditorLog
    {
        public static void Log(string log)
        {
            #if UNITY_EDITOR
            Debug.Log(log);
            #endif
        }

        public static void Warning(string log)
        {
            #if UNITY_EDITOR
            Debug.LogWarning(log);
            #endif
        }

        public static void Error(string log)
        {
            #if UNITY_EDITOR
            Debug.LogError(log);
            #endif
        }
    }
}
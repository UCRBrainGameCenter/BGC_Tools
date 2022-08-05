using BGC.UI.Dialogs;

namespace BGC.Scripting.Members
{
    public class DebugAdapter
    {
        public static void Log(string logString) => UnityEngine.Debug.Log(logString);
        public static void LogWarning(string logString) => UnityEngine.Debug.LogWarning(logString);
        public static void LogError(string logString) => UnityEngine.Debug.LogError(logString);

        public static void PopUp(string title, string body) => ModalDialog.ShowSimpleModal(
            mode: ModalDialog.Mode.Accept,
            headerText: title,
            bodyText: body);
    }
}

using UnityEngine;
using UnityEngine.Assertions;

namespace BGC.Utility.Unity
{
    public class ScreenShotOnButtonPress : MonoBehaviour
    {
        [SerializeField]
        private bool active = false;

        [SerializeField]
        private KeyCode keyCode = KeyCode.P;

        [SerializeField]
        private string screenShotTitle = "screenshot_";

        [SerializeField]
        private int screenShotIndex = 0;

        #if UNITY_EDITOR
        /// <summary>
        /// Check values that the user has assigned in the inspector
        /// </summary>
        private void Awake()
        {
            Assert.IsFalse(string.IsNullOrEmpty(screenShotTitle));

            if (keyCode == KeyCode.None)
            {
                Debug.LogWarning("Keycode for taking a screenshot is set to None and no screenshot can be taken.");
            }
        }

        /// <summary>
        /// Every frame check if the user has assigned the input to be checked
        /// and if so check the key. If the key is pressed then store a screen
        /// shot and log so the user knows the screenshot is saved
        /// </summary>
        private void Update()
        {
            if (active && Input.GetKeyUp(keyCode))
            {
                string title = $"{screenShotTitle}{screenShotIndex}.png";
                ScreenCapture.CaptureScreenshot(title);
                Debug.Log($"{title} screenshot saved");
                ++screenShotIndex;
            }
        }
        #endif
    }
}
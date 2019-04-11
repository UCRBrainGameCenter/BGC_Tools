using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace BGC.MonoUtility
{
    /// <summary>
    /// Used for server related tasks so we can inform the user on what is happening
    /// </summary>
    public class StatusPanel : MonoBehaviour
    {
        [SerializeField]
        private Text title = null;

        [SerializeField]
        private Text status = null;

        /// <summary> Title of the panel </summary>
        public string Title
        {
            get { return title.text; }
            set { title.text = value; }
        }

        /// <summary> Status content (body) of the panel </summary>
        public string Status
        {
            get { return status.text; }
            set { status.text = value; }
        }

        private void Awake()
        {
            Assert.IsNotNull(title);
            Assert.IsNotNull(status);
        }
    }
}
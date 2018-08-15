using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace BGC.MonoUtility
{
    /// <summary>
    /// Used for server related tasks so we can inform the user on what is 
    /// hapening
    /// </summary>
    public class StatusPanel : MonoBehaviour
    {
        [SerializeField]
        private Text title;

        [SerializeField]
        private Text status;

        /// <summary>
        /// Set and Get title of the panel
        /// </summary>
        public string Title
        {
            get
            {
                return title.text;
            }
            set
            {
                Assert.IsFalse(System.String.IsNullOrEmpty(value));
                title.text = value;
            }
        }

        /// <summary>
        /// Set and Get Status content in the panel
        /// </summary>
        public string Status
        {
            get
            {
                return status.text;
            }
            set
            {
                Assert.IsFalse(System.String.IsNullOrEmpty(value));
                status.text = value;
            }
        }

        private void Awake()
        {
            Assert.IsNotNull(title);
            Assert.IsNotNull(status);
        }
    }
}
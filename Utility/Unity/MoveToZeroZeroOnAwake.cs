using UnityEngine;

namespace BGC.Utility.Unity   
{
    public class MoveToZeroZeroOnAwake : MonoBehaviour
    {
        [SerializeField]
        private bool resetRecttransform = true;

        [SerializeField]
        private bool disableOnAwake = false;

        [SerializeField]
        private bool active = true;

        private void Awake()
        {
            if (active == true)
            {
                gameObject.transform.localPosition = Vector3.zero;

                if (resetRecttransform)
                {
                    RectTransform rt = GetComponent<RectTransform>();
                    rt.offsetMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;
                }

                if (disableOnAwake)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
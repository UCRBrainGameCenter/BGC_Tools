using UnityEngine;

namespace BGC.MonoUtility
{
    public class DestroyOnDestroy : MonoBehaviour
    {
        private void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}
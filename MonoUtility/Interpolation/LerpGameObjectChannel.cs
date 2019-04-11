using UnityEngine;

namespace BGC.MonoUtility.Interpolation
{
    /// <summary>
    /// Monobehavior to execute and manage animation-like actions on a GameObject.
    /// </summary>
    public class LerpGameObjectChannel : LerpChannel<GameObject>
    {
        protected override GameObject Target => gameObject;
    }
}
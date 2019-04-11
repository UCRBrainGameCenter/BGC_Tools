using UnityEngine;

namespace BGC.MonoUtility.Interaction
{
    public class ClickObjectChannel : ClickChannel<GameObject>
    {
        protected override GameObject Target => gameObject;
    }
}

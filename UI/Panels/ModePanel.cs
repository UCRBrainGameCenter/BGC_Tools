using System;
using UnityEngine;

namespace BGC.UI.Panels
{
#pragma warning disable UNT0007 // Null coalescing on Unity objects
    public abstract class ModePanel : MonoBehaviour
    {
        [NonSerialized] //Added to fix Unity Serialization issue
        private RectTransform rt = null;
        public RectTransform RectTransform => rt ?? (rt = GetComponent<RectTransform>());

        private RectTransform parentRT = null;
        public RectTransform ParentRectTransform => parentRT ?? (parentRT = RectTransform.parent.GetComponent<RectTransform>());

        [NonSerialized]
        private ModePanelLerpedActionChannel lerpHandler = null;
        public ModePanelLerpedActionChannel LerpHandler => lerpHandler ??
            (lerpHandler = GetComponent<ModePanelLerpedActionChannel>()) ??
            (lerpHandler = gameObject.AddComponent<ModePanelLerpedActionChannel>());

        public abstract void FocusAcquired();
        public abstract void FocusLost();

        public void ImmediateStateSet(bool visible)
        {
            RectTransform.pivot = ParentRectTransform.pivot;
            RectTransform.localPosition = visible ? Vector2.zero : new Vector2(Screen.width, Screen.height);
        }
    }
#pragma warning restore UNT0007 // Null coalescing on Unity objects
}

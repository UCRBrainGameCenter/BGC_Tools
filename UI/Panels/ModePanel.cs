using UnityEngine;

namespace BGC.UI.Panels
{
    public abstract class ModePanel : MonoBehaviour
    {
        private RectTransform _rt = null;
        public RectTransform RectTransform => _rt ?? (_rt = GetComponent<RectTransform>());

        private RectTransform _parentRT = null;
        public RectTransform ParentRectTransform => _parentRT ?? (_parentRT = RectTransform.parent.GetComponent<RectTransform>());

        public abstract void FocusAcquired();
        public abstract void FocusLost();

        public void ImmediateStateSet(bool visible)
        {
            RectTransform.pivot = ParentRectTransform.pivot;
            RectTransform.localPosition = visible ? Vector2.zero : new Vector2(Screen.width, Screen.height);
        }
        
        private ModePanelLerpedActionChannel _lerpHandler = null;
        public ModePanelLerpedActionChannel LerpHandler => _lerpHandler ??
            (_lerpHandler = GetComponent<ModePanelLerpedActionChannel>()) ??
            (_lerpHandler = gameObject.AddComponent<ModePanelLerpedActionChannel>());
    }
}


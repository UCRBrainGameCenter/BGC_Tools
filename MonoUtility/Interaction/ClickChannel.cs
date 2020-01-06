using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BGC.MonoUtility.Interaction
{
    public abstract class ClickChannel<T> : MonoBehaviour, IPointerClickHandler
    {
        protected abstract T Target { get; }
        protected Action<T> callback;

        /// <summary>
        /// Updates and overwrites current callback
        /// </summary>
        public void Activate(Action<T> callback)
        {
            this.callback = callback;
            
            Debug.Assert(!Target.Equals(default(T)));
        }

        public void StripCallback() => callback = null;
        
        protected virtual void OnClickPreCallback(PointerEventData eventData) { }
        protected virtual void OnClickPostCallback(PointerEventData eventData) { }

        #region IPointerClickHandler

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnClickPreCallback(eventData);
            callback?.Invoke(Target);
            OnClickPostCallback(eventData);
        }

        #endregion IPointerClickHandler
    }
}

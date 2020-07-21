using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BGC.Parameters.View
{
    public class LabelSwapTrigger : MonoBehaviour, IPointerClickHandler
    {
        protected float lastClickTime = 0f;
        protected const float doubleTapWindow = 1f;

        public delegate void DoubleClickHandler();

        public DoubleClickHandler OnDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time > lastClickTime + doubleTapWindow)
            {
                lastClickTime = Time.time;
            }
            else
            {
                //Handle doubleclick
                lastClickTime = 0f;

                if (OnDoubleClick != null)
                {
                    OnDoubleClick.Invoke();
                }

            }
        }
    }
}

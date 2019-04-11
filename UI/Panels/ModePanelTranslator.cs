using System;
using UnityEngine;
using BGC.MonoUtility.Interpolation;

namespace BGC.UI.Panels
{
    public enum Axis
    {
        XAxis = 0,
        YAxis
    }

    public enum Direction
    {
        Show = 0,
        Hide
    }

    public enum Orientation
    {
        Superior = 0,
        Inferior
    }

    public class ModePanelTranslator : ILerpAction<ModePanel>
    {
        private Vector2 initialPosition;
        private Vector2 finalPosition;
        private float buffer;

        private RectTransform rt = null;

        public ModePanelTranslator(Direction direction, Axis axis, Orientation orientation, float buffer = 10f)
        {
            Vector2 displacement;
            this.buffer = buffer;

            switch (axis)
            {
                case Axis.XAxis:
                    displacement = new Vector2(-1f, 0f);
                    break;

                case Axis.YAxis:
                    displacement = new Vector2(0f, 1f);
                    break;

                default:
                    Debug.LogError($"Axis unsupported: {axis}");
                    return;
            }

            switch (orientation)
            {
                case Orientation.Superior:
                    //Do Nothing
                    break;
                case Orientation.Inferior:
                    displacement *= -1f;
                    break;
                default:
                    Debug.LogError($"Orientation unsupported: {orientation}");
                    return;
            }

            switch (direction)
            {
                case Direction.Show:
                    initialPosition = displacement;
                    finalPosition = Vector2.zero;
                    break;
                case Direction.Hide:
                    initialPosition = Vector2.zero;
                    finalPosition = displacement;
                    break;
                default:
                    Debug.LogError($"Direction unsupported: {direction}");
                    return;
            }

        }

        void ILerpAction<ModePanel>.Initialize(ModePanel modelPanel)
        {
            rt = modelPanel.GetComponent<RectTransform>();

            RectTransform parent = rt.parent.GetComponent<RectTransform>();
            rt.pivot = parent.pivot;

            //Scale positions by the size, and add the local offset
            initialPosition.Scale(parent.rect.size + new Vector2(buffer, buffer));
            finalPosition.Scale(parent.rect.size + new Vector2(buffer, buffer));

            rt.localPosition = initialPosition;
        }

        void ILerpAction<ModePanel>.CallAction(float t)
        {
            rt.localPosition = Vector2.Lerp(initialPosition, finalPosition, Mathf.SmoothStep(0f, 1f, t));
        }
    }

}

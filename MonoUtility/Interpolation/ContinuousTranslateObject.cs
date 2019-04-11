using UnityEngine;

namespace BGC.MonoUtility.Interpolation
{
    public class ContinuousTranslation : IContinuousAction<GameObject>
    {
        private readonly Vector2 velocity;
        private float startTime;
        private Vector2 initialPosition;

        private RectTransform rt = null;

        public ContinuousTranslation(Vector2 velocity)
        {
            this.velocity = velocity;
            startTime = 0f;
        }

        void IContinuousAction<GameObject>.Initialize(GameObject gameObject, float time)
        {
            rt = gameObject.GetComponent<RectTransform>();
            initialPosition = rt.localPosition;
            startTime = time;
        }

        void IContinuousAction<GameObject>.CallAction(float time)
        {
            rt.localPosition = initialPosition + velocity * (time - startTime);
        }
    }
}
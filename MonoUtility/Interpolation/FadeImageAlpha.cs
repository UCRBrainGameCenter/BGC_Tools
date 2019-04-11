using UnityEngine;
using UnityEngine.UI;
using BGC.Mathematics;

namespace BGC.MonoUtility.Interpolation
{
    public class FadeImageAlpha : ILerpAction<Image>
    {
        private Image image = null;
        float initialAlpha;
        float finalAlpha;
        Color imageColor;

        public FadeImageAlpha(float initialAlpha = float.NaN, float finalAlpha = float.NaN)
        {
            this.initialAlpha = initialAlpha;
            this.finalAlpha = finalAlpha;
        }

        void ILerpAction<Image>.Initialize(Image target)
        {
            image = target;
            imageColor = image.color;

            //Load alpha if initialAlpha wasn't set
            if (float.IsNaN(initialAlpha))
            {
                initialAlpha = imageColor.a;
            }
            else
            {
                imageColor.a = initialAlpha;
                image.color = imageColor;
            }

            //Flip alpha if finalAlpha wasn't set
            if (float.IsNaN(finalAlpha))
            {
                finalAlpha = 1f - initialAlpha;
            }
        }

        void ILerpAction<Image>.CallAction(float t)
        {
            imageColor.a = GeneralMath.Lerp(initialAlpha, finalAlpha, t);
            image.color = imageColor;
        }

    }
}
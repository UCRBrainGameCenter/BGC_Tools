using System;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI.Panels
{
    /// <summary>
    /// Clones the appearance of a ModelPanel to show it sliding offscreen.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CopyPanel : ModePanel
    {
        [SerializeField]
        private Canvas mainCanvas = null;
        [SerializeField]
        private Camera mainCamera = null;

        [NonSerialized]
        private Image _image = null;
        public Image Image => _image ?? (_image = GetComponent<Image>());

        [NonSerialized]
        private RenderTexture rt;
        [NonSerialized]
        private Texture2D tex;

        private const float scalefactor = 4f;

        public void TakeSnapshot(RectTransform copyRect)
        {
            if (rt == null)
            {
                rt = new RenderTexture(
                    (int)(mainCanvas.pixelRect.width / scalefactor),
                    (int)(mainCanvas.pixelRect.height / scalefactor),
                    16);
            }

            if (tex == null)
            {
                tex = new Texture2D(
                    (int)(mainCamera.pixelWidth / scalefactor),
                    (int)(mainCamera.pixelHeight / scalefactor),
                    TextureFormat.RGB24, false);
            }


            Vector3[] corners = new Vector3[4];
            copyRect.GetWorldCorners(corners);

            Vector2 position = corners[0] / scalefactor;

            position.x = (float)Math.Floor(position.x);
            position.y = (float)Math.Floor(position.y);

            Vector2 size = (corners[2] - corners[0]) / scalefactor;

            size.x = (float)Math.Floor(size.x);
            size.y = (float)Math.Floor(size.y);

            Rect worldRect = new Rect(Vector2.zero, size);

            RenderMode cachedRenderMode = mainCanvas.renderMode;
            mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas.worldCamera = mainCamera;

            mainCamera.targetTexture = rt;

            mainCamera.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0f, 0f, tex.width, tex.height), 0, 0);
            tex.Apply();
            mainCamera.targetTexture = null;
            RenderTexture.active = null;

            mainCanvas.renderMode = cachedRenderMode;

            Image.overrideSprite = Sprite.Create(
                texture: tex,
                rect: worldRect,
                pivot: Vector2.zero);

        }

        public override void FocusAcquired()
        {
            //Do Nothing
        }

        public override void FocusLost()
        {
            //Do Nothing
        }
    }
}


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI.Panels
{
    /// <summary>
    /// Clones the appearance of a ModelPanel to show it sliding offscreen.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CopyPanel : ModePanel
    {
#pragma warning disable UNT0007 // Null coalescing on Unity objects

        private RawImage _image = null;
        public RawImage Image => _image ?? (_image = GetComponent<RawImage>());

#pragma warning restore UNT0007 // Null coalescing on Unity objects

        private RenderTexture previousFrameRT;
        private RenderTexture snapshotRT;

        void Start()
        {
            Image.enabled = false;
        }

        void Update()
        {
            StartCoroutine(CaptureScreen());
        }

        private IEnumerator CaptureScreen()
        {
            yield return new WaitForEndOfFrame();

            int rtWidth = Screen.width;
            int rtHeight = Screen.height;
            if (previousFrameRT == null || previousFrameRT.width != rtWidth || previousFrameRT.height != rtHeight)
            {
                if (previousFrameRT != null)
                {
                    Destroy(previousFrameRT);
                }
                previousFrameRT = new RenderTexture(rtWidth, rtHeight, 0);
            }
            ScreenCapture.CaptureScreenshotIntoRenderTexture(previousFrameRT);
        }

        public void TakeSnapshot()
        {
            int rtWidth = previousFrameRT.width;
            int rtHeight = previousFrameRT.height;
            if (snapshotRT == null || snapshotRT.width != rtWidth || snapshotRT.height != rtHeight)
            {
                if (snapshotRT != null)
                {
                    Destroy(snapshotRT);
                }
                snapshotRT = new RenderTexture(rtWidth, rtHeight, 0);
                Image.texture = snapshotRT;
            }
            Graphics.CopyTexture(previousFrameRT, snapshotRT);
        }

        public override void FocusAcquired()
        {
            // Show the image
            Image.enabled = true;
        }

        public override void FocusLost()
        {
            // Hide the image
            Image.enabled = false;
        }
    }
}


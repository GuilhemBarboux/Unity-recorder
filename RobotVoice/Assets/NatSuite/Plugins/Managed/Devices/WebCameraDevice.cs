/* 
*   NatDevice
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using UnityEngine;
    using System;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Camera device backed by a `WebCamTexture`.
    /// </summary>
    public sealed class WebCameraDevice : ICameraDevice {

        #region --Client API--
        /// <summary>
        /// The backing `WebCamTexture` device.
        /// </summary>
        public readonly WebCamDevice device;
        
        /// <summary>
        /// Device unique ID.
        /// </summary>
        public string uniqueID => device.name;

        /// <summary>
        /// Is the camera front facing?
        /// </summary>
        public bool frontFacing => device.isFrontFacing;

        /// <summary>
        /// Get or set the current preview resolution of the camera.
        /// </summary>
        public (int width, int height) previewResolution { get; set; }

        /// <summary>
        /// Get or set the current framerate of the camera.
        /// </summary>
        public int frameRate { get; set; }

        /// <summary>
        /// Is the device running?
        /// </summary>
        public bool running => webCamTexture.isPlaying;

        /// <summary>
        /// Start running.
        /// </summary>
        public Task<Texture2D> StartRunning () {
            var startTask = new TaskCompletionSource<Texture2D>();
            webCamTexture = new WebCamTexture(device.name, previewResolution.width, previewResolution.height, frameRate);
            webCamTexture.Play();
            attachment = new GameObject("NatDevice WebCameraDevice Helper").AddComponent<WebCameraDeviceAttachment>();
            attachment.@delegate = () => {
                // Check size // Weird macOS bug
                if (webCamTexture.width == 16 || webCamTexture.height == 16)
                    return;
                // Update preview texture
                var firstFrame = !previewTexture;
                previewTexture = previewTexture ?? new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false, false);
                pixelBuffer = pixelBuffer ?? webCamTexture.GetPixels32();
                webCamTexture.GetPixels32(pixelBuffer);
                previewTexture.SetPixels32(pixelBuffer);
                previewTexture.Apply();
                // Complete task
                if (firstFrame)
                    startTask.SetResult(previewTexture);
            };
            return startTask.Task;
        }

        /// <summary>
        /// Stop running.
        /// </summary>
        public void StopRunning () {
            attachment.@delegate = default;
            WebCameraDeviceAttachment.Destroy(attachment);
            webCamTexture.Stop();
            WebCamTexture.Destroy(webCamTexture);
            Texture2D.Destroy(previewTexture);
            webCamTexture = default;
            previewTexture = default;
            pixelBuffer = default;
            attachment = default;
        }
        #endregion


        #region --Operations--

        private WebCamTexture webCamTexture;
        private Texture2D previewTexture;
        private Color32[] pixelBuffer;
        private WebCameraDeviceAttachment attachment;

        public WebCameraDevice (WebCamDevice device) {
            this.device = device;
            this.previewResolution = (1280, 720);
            this.frameRate = 30;
        }

        public bool Equals (IMediaDevice other) => other != null && other is WebCameraDevice && other.uniqueID == uniqueID;

        public override string ToString () => $"webcam:{uniqueID}";

        private class WebCameraDeviceAttachment : MonoBehaviour {
            public Action @delegate;
            void Awake () => DontDestroyOnLoad(this.gameObject);
            void Update () => @delegate?.Invoke();
        }
        #endregion
    }
}
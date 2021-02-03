using System;
using NatSuite.Devices;
using UnityEngine;
using UnityEngine.UI;

namespace Record
{
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
    public class CameraReplay : MonoBehaviour
    {
        private ICameraDevice cameraDevice; // Microphone for recording user audio
        private RawImage cameraReplay;
        private AspectRatioFitter aspectFitter;
        
        private void Awake()
        {
            // Get a front camera
            var cameraQuery = new MediaDeviceQuery(MediaDeviceQuery.Criteria.FrontFacing);
            cameraDevice = cameraQuery.currentDevice as ICameraDevice;
        }

        private async void Start()
        {
            cameraReplay = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
            
            // Replay camera on imageRaw
            cameraReplay.texture = await cameraDevice.StartRunning();
            
            // Set aspect ratio
            aspectFitter.aspectRatio = (float) cameraReplay.texture.width / cameraReplay.texture.height;
        }
    }
}
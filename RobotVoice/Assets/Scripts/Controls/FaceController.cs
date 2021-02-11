using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace Controls
{
    
    [RequireComponent(typeof(ARFace))]
    public class FaceController : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern Quaternion GetFaceRotation(IntPtr ptr);
        private ARFace face;
        private RobotController[] robots;
        private ARSessionOrigin origin;

#if UNITY_IPHONE
        private ARKitFaceSubsystem arKitFaceSubsystem;
#endif
        private void Awake()
        {
            face = GetComponent<ARFace>();
            robots = FindObjectsOfType<RobotController>();
            origin = FindObjectOfType<ARSessionOrigin>();
        }

        private void OnEnable()
        {
            var faceManager = FindObjectOfType<ARFaceManager>();
            if (faceManager == null) return;

#if UNITY_IPHONE
            arKitFaceSubsystem = (ARKitFaceSubsystem) faceManager.subsystem;
#endif
            face.updated += OnFaceUpdated;
            ARSession.stateChanged += OnSessionStateChanged;
        }

        private void OnFaceUpdated(ARFaceUpdatedEventArgs arFaceUpdatedEventArgs)
        {
#if UNITY_IPHONE
            using var blendShapes = arKitFaceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp);
            var rotationRelativeToCamera = Quaternion.Inverse(origin.camera.transform.rotation) * face.transform.rotation; // this quaternion represents a face rotation relative to camera
            foreach (var robotController in robots)
            {
                robotController.SetBlendShapes(blendShapes);
                robotController.SetHeadRotation(rotationRelativeToCamera);
            }
#endif
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs arSessionStateChangedEventArgs)
        {
        }

        private void OnDisable()
        {
            face.updated -= OnFaceUpdated;
            ARSession.stateChanged -= OnSessionStateChanged;
        }
    }
}
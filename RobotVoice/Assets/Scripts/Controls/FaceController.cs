using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace Controls
{
    
    [RequireComponent(typeof(ARFace))]
    public class FaceController : MonoBehaviour
    {
        private ARFace face;
        private MeshController[] robots;
        private Menu menu;
        private ARSessionOrigin origin;

#if UNITY_IPHONE
        private ARKitFaceSubsystem arKitFaceSubsystem;
#endif
        private void Awake()
        {
            face = GetComponent<ARFace>();
            robots = FindObjectsOfType<MeshController>();
            menu = FindObjectOfType<Menu>();
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
            menu.HideHint();
        }

        private void OnFaceUpdated(ARFaceUpdatedEventArgs arFaceUpdatedEventArgs)
        {
#if UNITY_IPHONE
            using var blendShapes = arKitFaceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp);
            var rotationCamera = origin.camera.transform.rotation;
            var rotationRelativeToFace = Quaternion.Inverse(rotationCamera) * face.transform.rotation; // this quaternion represents a face rotation relative to camera
            // var rotationRelativeToOrigin = Quaternion.Inverse(rotationCamera) * origin.transform.rotation;
            
            foreach (var robotController in robots)
            {
                robotController.SetBlendShapes(blendShapes);
                robotController.SetHeadRotation(rotationRelativeToFace);
                // robotController.SetBodyRotation(rotationRelativeToOrigin);
            }
#endif
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs arSessionStateChangedEventArgs)
        {
            
        }

        private async void OnDisable()
        {
            face.updated -= OnFaceUpdated;
            ARSession.stateChanged -= OnSessionStateChanged;
            await Task.Delay(3000);
            // if (!enabled) menu.ShowHint();
        }
    }
}
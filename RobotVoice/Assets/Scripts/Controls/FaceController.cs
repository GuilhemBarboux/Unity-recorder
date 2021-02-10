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
        private RobotController[] robots;
        private Vector3 rotation;

#if UNITY_IPHONE
        private ARKitFaceSubsystem arKitFaceSubsystem;
#endif
        private void Awake()
        {
            face = GetComponent<ARFace>();
            robots = FindObjectsOfType<RobotController>();
            rotation = transform.rotation.eulerAngles;
            Debug.Log(rotation);
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
            foreach (var robotController in robots)
            {
                robotController.SetBlendShapes(blendShapes);
                robotController.SetHeadRotation(transform.rotation.eulerAngles);
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
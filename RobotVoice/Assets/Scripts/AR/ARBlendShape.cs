using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace AR
{
    [RequireComponent(typeof(ARFace))]
    public class ARBlendShape : MonoBehaviour
    {
        [FormerlySerializedAs("Blend Shape Map")] [SerializeField]
        private BlendShapeMap blendShapeMap;

        private ARFace face;

        protected void Awake()
        {
            face = GetComponent<ARFace>();

            if (!IsValid()) return;

#if UNITY_IPHONE
            foreach (var mapping in blendShapeMap.arkitMap)
            {
                var blendShapeIndex = $"{blendShapeMap.prefixMap}.{mapping.name}";
                faceArkitBlendShapeIndexMap[mapping.location] =
                    GetShapeIndex(blendShapeIndex);
            }
#endif
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

        private void OnDisable()
        {
            face.updated -= OnFaceUpdated;
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        private void OnFaceUpdated(ARFaceUpdatedEventArgs arFaceUpdatedEventArgs)
        {
            if (!IsValid()) return;
            
#if UNITY_IPHONE
            using var blendShapes = arKitFaceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp);
            foreach (var blendShape in blendShapes)
            {
                if (!faceArkitBlendShapeIndexMap.TryGetValue(blendShape.blendShapeLocation,
                    out var mappedBlendShapeIndex)) continue;

                if (mappedBlendShapeIndex >= 0)
                    SetShapeWeight(mappedBlendShapeIndex, blendShape.coefficient);
            }
#endif
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs arSessionStateChangedEventArgs)
        {
        }

        // Overrides
        protected virtual bool IsValid()
        {
            return true;
        }

        protected virtual void SetShapeWeight(int index, float weight)
        {
        }

        protected virtual int GetShapeIndex(string id)
        {
            return 0;
        }


#if UNITY_IPHONE
        private ARKitFaceSubsystem arKitFaceSubsystem;
        private readonly Dictionary<ARKitBlendShapeLocation, int> faceArkitBlendShapeIndexMap =
            new Dictionary<ARKitBlendShapeLocation, int>();
#endif
    }
}
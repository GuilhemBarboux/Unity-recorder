using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace AR
{
    [RequireComponent(typeof(ARFace))]
    public class BlendShape : MonoBehaviour
    {
        [FormerlySerializedAs("Blend Shape Map")] [SerializeField]
        private BlendShapeMap blendShapeMap;

        [FormerlySerializedAs("Mesh")] [SerializeField]
        private SkinnedMeshRenderer meshRenderer;

        private ARFace _face;

        private void Awake()
        {
            _face = GetComponent<ARFace>();

            if (!IsMeshValid()) return;

#if UNITY_IPHONE
            foreach (var mapping in blendShapeMap.arkitMap)
            {
                var blendShapeIndex = $"{blendShapeMap.prefixMap}.{mapping.name}";
                _faceArkitBlendShapeIndexMap[mapping.location] =
                    meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeIndex);
            }
#endif
        }

        private void OnEnable()
        {
            var faceManager = FindObjectOfType<ARFaceManager>();
            if (faceManager == null) return;
            
#if UNITY_IPHONE
            _arKitFaceSubsystem = (ARKitFaceSubsystem) faceManager.subsystem;
#endif
            _face.updated += OnFaceUpdated;
            ARSession.stateChanged += OnSessionStateChanged;

        }

        private void OnDisable()
        {
            _face.updated -= OnFaceUpdated;
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        private void OnFaceUpdated(ARFaceUpdatedEventArgs arFaceUpdatedEventArgs)
        {
            if (!IsMeshValid()) return;
            
            
#if UNITY_IPHONE
            using var blendShapes = _arKitFaceSubsystem.GetBlendShapeCoefficients(_face.trackableId, Allocator.Temp);
            foreach (var blendShape in blendShapes)
            {
                if (!_faceArkitBlendShapeIndexMap.TryGetValue(blendShape.blendShapeLocation,
                    out var mappedBlendShapeIndex)) continue;

                if (mappedBlendShapeIndex >= 0)
                    meshRenderer.SetBlendShapeWeight(mappedBlendShapeIndex, blendShape.coefficient);
            }
#endif
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs arSessionStateChangedEventArgs)
        {
        }

        private bool IsMeshValid()
        {
            return meshRenderer != null && meshRenderer.enabled && meshRenderer.sharedMesh != null;
        }


#if UNITY_IPHONE
        private ARKitFaceSubsystem _arKitFaceSubsystem;
        private readonly Dictionary<ARKitBlendShapeLocation, int> _faceArkitBlendShapeIndexMap =
            new Dictionary<ARKitBlendShapeLocation, int>();
#endif
    }
}
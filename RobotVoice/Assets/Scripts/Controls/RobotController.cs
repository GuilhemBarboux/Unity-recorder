using System;
using System.Collections.Generic;
using System.Linq;
using AR;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace Controls
{
    [RequireComponent(typeof(ARFace))]
    public class RobotController : MonoBehaviour
    {
        // Robot meshs
        [SerializeField] private Transform eyeRight;
        [SerializeField] private Transform eyeLeft;
        [SerializeField] private Transform mouthUp;
        [SerializeField] private Transform mouthDown;

        // Robot material
        [SerializeField] private Material eyeRightMaterial;
        [SerializeField] private Material eyeLeftMaterial;

        // Movement coefficients
        [SerializeField] private float eyeRotationCoefficient = 8;
        [SerializeField] private float mouthRotationCoefficient = 16;

        [SerializeField]
        [Range(0, 1)]
        private float eyeIntensityMin = 0;
        
        // Blend shapes from ARKIT
        private ARFace face;
        public readonly Dictionary<ARKitBlendShapeLocation, float> shapeWeights = new Dictionary<ARKitBlendShapeLocation, float>
        {
            // Left eye
            { ARKitBlendShapeLocation.EyeLookUpLeft, 0 },
            { ARKitBlendShapeLocation.EyeLookDownLeft, 0 },
            { ARKitBlendShapeLocation.EyeLookInLeft, 0 },
            { ARKitBlendShapeLocation.EyeLookOutLeft, 0 },
            // Right eye
            { ARKitBlendShapeLocation.EyeLookUpRight, 0 },
            { ARKitBlendShapeLocation.EyeLookDownRight, 0 },
            { ARKitBlendShapeLocation.EyeLookInRight, 0 },
            { ARKitBlendShapeLocation.EyeLookOutRight, 0 },
            // Blink
            { ARKitBlendShapeLocation.EyeBlinkLeft, 0 },
            { ARKitBlendShapeLocation.EyeBlinkRight, 0 },
            
            // Jaw open
            { ARKitBlendShapeLocation.JawOpen, 0 },
            { ARKitBlendShapeLocation.MouthClose, 0 }
        };
#if UNITY_IPHONE
        private ARKitFaceSubsystem arKitFaceSubsystem;
#endif
        
        // Inital states
        private Quaternion eyeLeftRotation;
        private Quaternion eyeRightRotation;
        private Quaternion mouthUpRotation;
        private Quaternion mouthDownRotation;
        private Color eyeLeftColor;
        private Color eyeRightColor;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            face = GetComponent<ARFace>();
            
            // Initial eyes rotations
            var leftRotation = eyeLeft.rotation;
            eyeLeftRotation = new Quaternion(leftRotation.x, leftRotation.y, leftRotation.z, leftRotation.w);
            var rightRotation = eyeRight.rotation;
            eyeRightRotation = new Quaternion(rightRotation.x, rightRotation.y, rightRotation.z, rightRotation.w);
            
            // Initial eyes emissive color
            eyeLeftColor = eyeLeftMaterial.GetColor(EmissionColor);
            eyeRightColor = eyeRightMaterial.GetColor(EmissionColor);
            
            // Initial mouth rotations
            var muRotation = mouthUp.rotation;
            mouthUpRotation = new Quaternion(muRotation.x, muRotation.y, muRotation.z, muRotation.w);
            var mdRotation = mouthDown.rotation;
            mouthDownRotation = new Quaternion(mdRotation.x, mdRotation.y, mdRotation.z, mdRotation.w);
        }

        private void OnDestroy()
        {
            eyeLeftMaterial.SetColor(EmissionColor, eyeLeftColor);
            eyeRightMaterial.SetColor(EmissionColor, eyeRightColor);
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
#if UNITY_IPHONE
            using var blendShapes = arKitFaceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp);
            foreach (var blendShape in blendShapes.Where(blendShape => shapeWeights.ContainsKey(blendShape.blendShapeLocation)))
            {
                shapeWeights[blendShape.blendShapeLocation] = blendShape.coefficient;
            }
#endif
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs arSessionStateChangedEventArgs)
        {
        }

        private void Update()
        {
            // Eyes rotations
            var leftEyeZ = shapeWeights[ARKitBlendShapeLocation.EyeLookOutLeft] -
                           shapeWeights[ARKitBlendShapeLocation.EyeLookInLeft];
            var rightEyeZ = shapeWeights[ARKitBlendShapeLocation.EyeLookInRight] -
                            shapeWeights[ARKitBlendShapeLocation.EyeLookOutRight];
            
            var leftEyeX = shapeWeights[ARKitBlendShapeLocation.EyeLookDownLeft] -
                       shapeWeights[ARKitBlendShapeLocation.EyeLookUpLeft];
            var rightEyeX = shapeWeights[ARKitBlendShapeLocation.EyeLookDownRight] -
                           shapeWeights[ARKitBlendShapeLocation.EyeLookUpRight];

            eyeLeft.rotation = eyeLeftRotation;
            eyeLeft.Rotate(360 + leftEyeX * eyeRotationCoefficient, 0, 360 + leftEyeZ * eyeRotationCoefficient);
            
            eyeRight.rotation = eyeRightRotation;
            eyeRight.Rotate(360 + rightEyeX * eyeRotationCoefficient, 0, 360 + rightEyeZ * eyeRotationCoefficient);

            // Eyes colors
            var leftIntensity = eyeIntensityMin +
                                (1 - eyeIntensityMin) * shapeWeights[ARKitBlendShapeLocation.EyeBlinkLeft];
            var rightIntensity = eyeIntensityMin +
                                (1 - eyeIntensityMin) * shapeWeights[ARKitBlendShapeLocation.EyeBlinkRight];
            eyeLeftMaterial.SetColor(EmissionColor, eyeLeftColor * leftIntensity);
            eyeRightMaterial.SetColor(EmissionColor, eyeRightColor * rightIntensity);
            
            // Mouth
            var mouseOpen = Math.Max(shapeWeights[ARKitBlendShapeLocation.JawOpen] -
                                     shapeWeights[ARKitBlendShapeLocation.MouthClose], 0);
            mouthUp.rotation = mouthUpRotation;
            mouthUp.Rotate(360 - mouseOpen * mouthRotationCoefficient * 0.2f, 0, 0);
            mouthDown.rotation = mouthDownRotation;
            mouthDown.Rotate(360 + mouseOpen * mouthRotationCoefficient, 0, 0);
        }
    }
}

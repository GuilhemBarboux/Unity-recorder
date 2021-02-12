using System;
using System.Collections;
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
    public class RobotController : MonoBehaviour
    {
        // Robot meshs
        [SerializeField] private Transform eyeRight;
        [SerializeField] private Transform eyeLeft;
        [SerializeField] private Transform mouthUp;
        [SerializeField] private Transform mouthDown;
        [SerializeField] public Transform head;
        [SerializeField] public Transform body;
        [SerializeField] public Transform neck;

        // Robot material
        [SerializeField] private Material eyeRightMaterial;
        [SerializeField] private Material eyeLeftMaterial;

        // Movement coefficients
        [SerializeField] public float eyeRotationCoefficient = 20;
        [SerializeField] public float mouthRotationCoefficient = 32;
        [SerializeField] [Range(0, 10)] 
        public float intensityCoefficient = 1f;
        [SerializeField] [Range(0, 10)] 
        public float eyeIntensityMin = 9f;
        [SerializeField] [Range(0, 1)] 
        public float eyeInRotationCoefficient = 0.6f;
        [SerializeField] [Range(0, 1)] 
        public float mouthUpCoefficient = 0.6f;

        // Blend shapes from ARKIT
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
        
        // Inital states
        private Quaternion eyeLeftRotation;
        private Quaternion eyeRightRotation;
        private Quaternion mouthUpRotation;
        private Quaternion mouthDownRotation;
        private Quaternion headRotation;
        private Quaternion neckRotation;
        private Quaternion bodyRotation;
        private Color eyeLeftColor;
        private Color eyeRightColor;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            // Initial eyes rotations
            var leftRotation = eyeLeft.localRotation;
            eyeLeftRotation = new Quaternion(leftRotation.x, leftRotation.y, leftRotation.z, leftRotation.w);
            var rightRotation = eyeRight.localRotation;
            eyeRightRotation = new Quaternion(rightRotation.x, rightRotation.y, rightRotation.z, rightRotation.w);
            
            // Initial eyes emissive color
            eyeLeftMaterial.EnableKeyword("_EMISSION");
            eyeRightMaterial.EnableKeyword("_EMISSION");
            eyeLeftColor = eyeLeftMaterial.GetColor(EmissionColor);
            eyeRightColor = eyeRightMaterial.GetColor(EmissionColor);
            
            // Initial mouth rotations
            var muRotation = mouthUp.localRotation;
            mouthUpRotation = new Quaternion(muRotation.x, muRotation.y, muRotation.z, muRotation.w);
            var mdRotation = mouthDown.localRotation;
            mouthDownRotation = new Quaternion(mdRotation.x, mdRotation.y, mdRotation.z, mdRotation.w);
            
            // Initial head rotation
            var hRotation = head.localRotation;
            headRotation = new Quaternion(hRotation.x, hRotation.y, hRotation.z, hRotation.w);
            var nRotation = neck.localRotation;
            neckRotation = new Quaternion(nRotation.x, nRotation.y, nRotation.z, nRotation.w);
            var bRotation = body.localRotation;
            bodyRotation = new Quaternion(bRotation.x, bRotation.y, bRotation.z, bRotation.w);
        }
        
        /* private IEnumerator Start() {
            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                // Start some fallback experience for unsupported devices
                Debug.Log("AR session isn't supported");
            }
            else
            {
                // Start the AR session
                session.enabled = true;
                Debug.Log("AR session is " + ARSession.state);
            }
        } */

        private void OnDestroy()
        {
            eyeLeftMaterial.SetColor(EmissionColor, eyeLeftColor);
            eyeRightMaterial.SetColor(EmissionColor, eyeRightColor);
        }

        public void SetBlendShapes(NativeArray<ARKitBlendShapeCoefficient> blendShapes)
        {
            foreach (var blendShape in blendShapes.Where(blendShape => shapeWeights.ContainsKey(blendShape.blendShapeLocation)))
            {
                shapeWeights[blendShape.blendShapeLocation] = blendShape.coefficient;
            }
        }

        public void SetHeadRotation(Quaternion rotation)
        {
            var inverse = Quaternion.Inverse(rotation);
            head.localRotation = headRotation * inverse;
            neck.localRotation = neckRotation * Quaternion.Slerp(inverse, Quaternion.identity, 0.75f);
            body.localRotation = bodyRotation * Quaternion.Slerp(inverse, Quaternion.identity, 0.95f);
        }

        private void Update()
        {
            // Eyes rotations
            var leftEyeZ = shapeWeights[ARKitBlendShapeLocation.EyeLookOutLeft] -
                           shapeWeights[ARKitBlendShapeLocation.EyeLookInLeft] * eyeInRotationCoefficient;
            var rightEyeZ = shapeWeights[ARKitBlendShapeLocation.EyeLookInRight] * eyeInRotationCoefficient -
                            shapeWeights[ARKitBlendShapeLocation.EyeLookOutRight];
            
            var leftEyeX = shapeWeights[ARKitBlendShapeLocation.EyeLookDownLeft] -
                       shapeWeights[ARKitBlendShapeLocation.EyeLookUpLeft];
            var rightEyeX = shapeWeights[ARKitBlendShapeLocation.EyeLookDownRight] -
                           shapeWeights[ARKitBlendShapeLocation.EyeLookUpRight];
            
            eyeLeft.localRotation = eyeLeftRotation; // z because eyes rig is rotate
            eyeLeft.Rotate(360 + leftEyeX * eyeRotationCoefficient, 0, 360 + leftEyeZ * eyeRotationCoefficient);
            
            eyeRight.localRotation = eyeRightRotation; // z because eyes rig is rotate
            eyeRight.Rotate(360 + rightEyeX * eyeRotationCoefficient, 0, 360 + rightEyeZ * eyeRotationCoefficient);

            // Eyes colors
            // var leftIntensity =  (1f - shapeWeights[ARKitBlendShapeLocation.EyeBlinkLeft]) * intensityCoefficient;
            // var rightIntensity = (1f - shapeWeights[ARKitBlendShapeLocation.EyeBlinkRight]) * intensityCoefficient;
            // Debug.Log(eyeIntensityMin + " " + leftIntensity);
            // eyeLeftMaterial.SetColor(EmissionColor, eyeLeftColor * (eyeIntensityMin + leftIntensity));
            // eyeRightMaterial.SetColor(EmissionColor, eyeRightColor * (eyeIntensityMin + intensityCoefficient));
            
            // Mouth
            var mouseOpen = Mathf.Max(shapeWeights[ARKitBlendShapeLocation.JawOpen] -
                                     shapeWeights[ARKitBlendShapeLocation.MouthClose], 0);
            mouthUp.localRotation = mouthUpRotation;
            mouthUp.Rotate(360 - mouseOpen * mouthRotationCoefficient * mouthUpCoefficient, 0, 0);
            mouthDown.localRotation = mouthDownRotation;
            mouthDown.Rotate(360 + mouseOpen * mouthRotationCoefficient, 0, 0);
        }
    }
}

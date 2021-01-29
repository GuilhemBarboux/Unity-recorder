using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARKit;

namespace AR
{
    [Serializable]
    public class Mapping
    {
        [FormerlySerializedAs("Location")] public ARKitBlendShapeLocation location;

        [FormerlySerializedAs("Name")] public string name;
    }

    [CreateAssetMenu(fileName = "BlendShapeMap", menuName = "AR/Blend Shape Map", order = 1)]
    public class BlendShapeMap : ScriptableObject
    {
        [SerializeField] public string prefixMap;
        [SerializeField] public float coefficientScale = 100.0f;

        [FormerlySerializedAs("ARKit Map")] [SerializeField]
        public List<Mapping> arkitMap = new List<Mapping>
        {
            new Mapping {location = ARKitBlendShapeLocation.BrowDownLeft, name = "BrowDownLeft"},
            new Mapping {location = ARKitBlendShapeLocation.BrowDownRight, name = "BrowDownRight"},
            new Mapping {location = ARKitBlendShapeLocation.BrowInnerUp, name = "BrowInnerUp"},
            new Mapping {location = ARKitBlendShapeLocation.BrowOuterUpLeft, name = "BrowOuterUpLeft"},
            new Mapping {location = ARKitBlendShapeLocation.BrowOuterUpRight, name = "BrowOuterUpRight"},
            new Mapping {location = ARKitBlendShapeLocation.CheekPuff, name = "CheekPuff"},
            new Mapping {location = ARKitBlendShapeLocation.CheekSquintLeft, name = "CheekSquintLeft"},
            new Mapping {location = ARKitBlendShapeLocation.CheekSquintRight, name = "CheekSquintRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeBlinkLeft, name = "EyeBlinkLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeBlinkRight, name = "EyeBlinkRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookDownLeft, name = "EyeLookDownLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookDownRight, name = "EyeLookDownRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookInLeft, name = "EyeLookInLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookInRight, name = "EyeLookInRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookOutLeft, name = "EyeLookOutLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookOutRight, name = "EyeLookOutRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookUpLeft, name = "EyeLookUpLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeLookUpRight, name = "EyeLookUpRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeSquintLeft, name = "EyeSquintLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeSquintRight, name = "EyeSquintRight"},
            new Mapping {location = ARKitBlendShapeLocation.EyeWideLeft, name = "EyeWideLeft"},
            new Mapping {location = ARKitBlendShapeLocation.EyeWideRight, name = "EyeWideRight"},
            new Mapping {location = ARKitBlendShapeLocation.JawForward, name = "JawForward"},
            new Mapping {location = ARKitBlendShapeLocation.JawLeft, name = "JawLeft"},
            new Mapping {location = ARKitBlendShapeLocation.JawOpen, name = "JawOpen"},
            new Mapping {location = ARKitBlendShapeLocation.JawRight, name = "JawRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthClose, name = "MouthClose"},
            new Mapping {location = ARKitBlendShapeLocation.MouthDimpleLeft, name = "MouthDimpleLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthDimpleRight, name = "MouthDimpleRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthFrownLeft, name = "MouthFrownLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthFrownRight, name = "MouthFrownRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthFunnel, name = "MouthFunnel"},
            new Mapping {location = ARKitBlendShapeLocation.MouthLeft, name = "MouthLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthLowerDownLeft, name = "MouthLowerDownLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthLowerDownRight, name = "MouthLowerDownRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthPressLeft, name = "MouthPressLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthPressRight, name = "MouthPressRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthPucker, name = "MouthPucker"},
            new Mapping {location = ARKitBlendShapeLocation.MouthRight, name = "MouthRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthRollLower, name = "MouthRollLower"},
            new Mapping {location = ARKitBlendShapeLocation.MouthRollUpper, name = "MouthRollUpper"},
            new Mapping {location = ARKitBlendShapeLocation.MouthShrugLower, name = "MouthShrugLower"},
            new Mapping {location = ARKitBlendShapeLocation.MouthShrugUpper, name = "MouthShrugUpper"},
            new Mapping {location = ARKitBlendShapeLocation.MouthSmileLeft, name = "MouthSmileLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthSmileRight, name = "MouthSmileRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthStretchLeft, name = "MouthStretchLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthStretchRight, name = "MouthStretchRight"},
            new Mapping {location = ARKitBlendShapeLocation.MouthUpperUpLeft, name = "MouthUpperUpLeft"},
            new Mapping {location = ARKitBlendShapeLocation.MouthUpperUpRight, name = "MouthUpperUpRight"},
            new Mapping {location = ARKitBlendShapeLocation.NoseSneerLeft, name = "NoseSneerLeft"},
            new Mapping {location = ARKitBlendShapeLocation.NoseSneerRight, name = "NoseSneerRight"},
            new Mapping {location = ARKitBlendShapeLocation.TongueOut, name = "TongueOut"}
        };
    }
}
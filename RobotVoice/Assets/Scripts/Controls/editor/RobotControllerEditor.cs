using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARKit;

namespace Controls.editor
{
    [CustomEditor(typeof(RobotController))]
    public class RobotControllerEditor : UnityEditor.Editor
    {
        private RobotController ctrl;
        private readonly Dictionary<ARKitBlendShapeLocation, float> shapeValues = new Dictionary<ARKitBlendShapeLocation, float>();
        private void OnEnable()
        {
            ctrl = (RobotController) target;
            foreach (var ctrlShapeWeight in ctrl.shapeWeights)
            {
                shapeValues.Add(ctrlShapeWeight.Key, (int)(ctrlShapeWeight.Value * 100));
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.LabelField("ARkit debug", EditorStyles.boldLabel);
            
            foreach (var weight in ctrl.shapeWeights)
            {
                shapeValues[weight.Key] = EditorGUILayout.Slider(weight.Key.ToString(), weight.Value, 0, 1);
            }
            
            foreach (var weight in shapeValues)
            {
                // ReSharper disable once PossibleLossOfFraction
                ctrl.shapeWeights[weight.Key] = weight.Value;
            }
        }
    }
}

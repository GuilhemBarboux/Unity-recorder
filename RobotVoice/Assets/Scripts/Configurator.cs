using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Controls;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

public class Configurator : MonoBehaviour
{
    [SerializeField] private RobotController robot;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject indicator;
    [SerializeField] private GameObject view;
    [SerializeField] private Slider mouth;
    [SerializeField] private Text mouthValue;
    [SerializeField] private Slider eyes;
    [SerializeField] private Text eyesValue;
    [SerializeField] private Slider intensity;
    [SerializeField] private Text intensityValue;
    [SerializeField] private Transform headRotation;
    [SerializeField] private Transform intialHeadRotation;
    [SerializeField] private Vector3 headMove;
    [SerializeField] private Vector3 bodyMove;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RawImage display;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private ARFace face;
    [SerializeField] private ARSessionOrigin origin;

    private readonly Dictionary<ARKitBlendShapeLocation, Indicator> indicators =
        new Dictionary<ARKitBlendShapeLocation, Indicator>();

    private void Awake()
    {
        view.SetActive(false);
    }

    private void Start()
    {
        mouth.value = robot.mouthRotationCoefficient;
        eyes.value = robot.eyeRotationCoefficient;
        intensity.value = robot.intensityCoefficient * 10f;
        intialHeadRotation.rotation = robot.head.rotation;
        headRotation.rotation = robot.head.localRotation;
        
        foreach (var robotShapeWeight in robot.shapeWeights)
        {
            var n = robotShapeWeight.Key.ToString();
            var i = Instantiate(indicator, panel.transform, true);
            var fields = i.GetComponent<Indicator>();

            i.name = n;
            fields.label.text = n;
            
            indicators.Add(robotShapeWeight.Key, fields);
        }
    }

    public void UpdateMouth()
    {
        if (robot == null) return;
        robot.mouthRotationCoefficient = mouth.value;
        mouthValue.text = mouth.value.ToString(CultureInfo.InvariantCulture);
    }

    public void UpdateEyes()
    {
        if (robot == null) return;
        robot.eyeRotationCoefficient = eyes.value;
        eyesValue.text = eyes.value.ToString(CultureInfo.InvariantCulture);
    }

    public void UpdateIntensity()
    {
        if (robot == null) return;
        robot.intensityCoefficient = intensity.value / 10;
        intensityValue.text = intensity.value.ToString(CultureInfo.InvariantCulture);
    }

    private void Update()
    {
        
#if UNITY_EDITOR
        foreach (var robotShapeWeight in robot.shapeWeights)
        {
            indicators[robotShapeWeight.Key].value.text = robotShapeWeight.Value.ToString(CultureInfo.InvariantCulture);
        }
#endif
        
        /* var frameBuffer = RenderTexture.GetTemporary(new RenderTextureDescriptor(711, 400, RenderTextureFormat.ARGBFloat, 24));
        var prevTarget = renderCamera.targetTexture;
        renderCamera.targetTexture = frameBuffer;
        renderCamera.Render();
        renderCamera.targetTexture = prevTarget;
        Graphics.Blit(frameBuffer, renderTexture);
        RenderTexture.ReleaseTemporary(frameBuffer); */

        /* var prevActive = RenderTexture.active;
        var activeTexture = renderCamera.targetTexture;
        RenderTexture.active = activeTexture;
        renderTexture.ReadPixels(new Rect(0, 0, activeTexture.width, activeTexture.height), 0, 0, false);
        RenderTexture.active = prevActive; */
    }

    public void Toggle()
    {
        view.SetActive(!view.activeSelf);
    }
}

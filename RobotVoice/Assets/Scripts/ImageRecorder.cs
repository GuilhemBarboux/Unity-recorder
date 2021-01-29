using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRecorder : MonoBehaviour
{
    #region Fields

    [SerializeField]
    public RenderTexture renderTexture;
    [SerializeField]
    private RawImage captureRawImage;
    [SerializeField]
    private Camera cameraRenderTexture;
    [SerializeField]
    private Vector2Int[] resolutions;

    [HideInInspector]
    public bool inFrameCommit;
    [HideInInspector]
    public Vector2Int videoCaptureResolution = new Vector2Int(-1, -1);
    private Texture2D previewTexture;
    private Texture2D readBackTexture;
    private List<Texture2D> videoFrames;

    #endregion

    #region Public Methods

    public async void StartCameraCapture()
    {
        Manager.singleton.cameraDevice.frameRate = 30;

        Vector2Int resolutionCam = new Vector2Int();
#if UNITY_IPHONE
        resolutionCam.y = Manager.singleton.cameraDevice.previewResolution.width;
        resolutionCam.x = Manager.singleton.cameraDevice.previewResolution.height;
#else
        resolutionCam.x = Manager.singleton.cameraDevice.previewResolution.width;
        resolutionCam.y = Manager.singleton.cameraDevice.previewResolution.height;
#endif
        Manager.singleton.ShowInfos(InfosType.cameraResolution, resolutionCam.x + "x" + resolutionCam.y);

        previewTexture = await Manager.singleton.cameraDevice.StartRunning();
        float ratio = previewTexture.height / 230;
        captureRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, previewTexture.width/ratio);
        captureRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 230/*previewTexture.height*/);
        captureRawImage.texture = previewTexture;
        Manager.singleton.ShowInfos(InfosType.backgroundResolution, captureRawImage.rectTransform.sizeDelta.x + "x" + captureRawImage.rectTransform.sizeDelta.y);
        RenderTexture rt = new RenderTexture(renderTexture.width, renderTexture.height, renderTexture.depth);
        cameraRenderTexture.targetTexture = rt;
        readBackTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        videoCaptureResolution.x = renderTexture.width;
        videoCaptureResolution.y = renderTexture.height;
        Manager.singleton.ShowInfos(InfosType.captureResolution, videoCaptureResolution.x+"x"+videoCaptureResolution.y);
    }

    public void StartRecording()
    {
        videoFrames = new List<Texture2D>();
        
    }

    public void Recording()
    {
        cameraRenderTexture.Render();
        RenderTexture.active = cameraRenderTexture.targetTexture;        
        readBackTexture.ReadPixels(new Rect(0, 0, cameraRenderTexture.targetTexture.width, cameraRenderTexture.targetTexture.height), 0, 0);
        RenderTexture.active = null;
        Texture2D copyOfTexture = new Texture2D(readBackTexture.width, readBackTexture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(readBackTexture, copyOfTexture);
        videoFrames.Add(copyOfTexture);
    }

    public IEnumerator FinishRecord(MP4Recorder recorder, FixedIntervalClock fixedIntervalClock)
    {
        inFrameCommit = true;
        foreach (Texture2D frame in videoFrames)
        {
            recorder.CommitFrame(frame.GetPixels32(), fixedIntervalClock.timestamp);
            Destroy(frame);
            yield return null;
        }
        inFrameCommit = false;
    }

    public void VideoDispose()
    {
        videoFrames = null;
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    public void ValueChangeCheck(float value)
    {
        Vector2Int chosenRes = resolutions[(int)value];
        RenderTexture rt = new RenderTexture(chosenRes.x, chosenRes.y, renderTexture.depth);
        cameraRenderTexture.targetTexture = rt;
        videoCaptureResolution.x = rt.width;
        videoCaptureResolution.y = rt.height;
        readBackTexture = new Texture2D(videoCaptureResolution.x, videoCaptureResolution.y, TextureFormat.RGB24, false);
        Manager.singleton.ShowInfos(InfosType.captureResolution, videoCaptureResolution.x + "x" + videoCaptureResolution.y);
        Manager.singleton.ReloadRecorder();
    }

    #endregion
}
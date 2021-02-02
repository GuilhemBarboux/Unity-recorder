using System;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Export
{
    public readonly int x;
    public readonly int y;
    public readonly MP4Recorder recorder;

    public Export(MP4Recorder recorder, Texture target)
    {
        this.recorder = recorder;
        // ReSharper disable once PossibleLossOfFraction
        x = Mathf.FloorToInt((target.width - recorder.frameSize.width) / 2);
        // ReSharper disable once PossibleLossOfFraction
        y = Mathf.FloorToInt((target.height - recorder.frameSize.height) / 2);
    }
}

public class ImageRecorder : MonoBehaviour
{
    #region Fields

    [SerializeField]
    public RenderTexture renderTexture;
    [SerializeField]
    private RawImage captureRawImage;
    [SerializeField]
    private Camera cameraRenderTexture;
    // [SerializeField]
    // private Vector2Int[] resolutions;

    [HideInInspector]
    public bool inFrameCommit;
    [HideInInspector]
    public Vector2Int videoCaptureResolution = new Vector2Int(-1, -1);
    private Texture2D _previewTexture;
    private Texture2D _readBackTexture;
    private List<Texture2D> _videoFrames;

    #endregion

    #region Public Methods

    public async void StartCameraCapture(IEnumerable<ExportResolution> resolutions)
    {
        // Initialize Camera
        Manager.singleton.cameraDevice.frameRate = 30;

        // Debug : display camera resolution
        videoCaptureResolution = new Vector2Int
        {
#if UNITY_IPHONE
            y = Manager.singleton.cameraDevice.previewResolution.width,
            x = Manager.singleton.cameraDevice.previewResolution.height
#else
            x = Manager.singleton.cameraDevice.previewResolution.width,
            y = Manager.singleton.cameraDevice.previewResolution.height
#endif
        };
        Manager.singleton.ShowInfos(InfosType.cameraResolution, videoCaptureResolution.x + "x" + videoCaptureResolution.y);

        // Set position of camera
        _previewTexture = await Manager.singleton.cameraDevice.StartRunning();
        var ratio = _previewTexture.height / 230;
        captureRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _previewTexture.width / ratio);
        captureRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 230/*previewTexture.height*/);
        captureRawImage.texture = _previewTexture;

        // Initialize camera render texture
        var renderResolution = new Vector2Int(0, 0);
        foreach (var exportResolution in resolutions)
        {
            var dimension = exportResolution.dimension;
            if (dimension[0] > renderResolution[0]) renderResolution[0] = dimension[0];
            if (dimension[1] > renderResolution[1]) renderResolution[1] = dimension[1];
        }
        cameraRenderTexture.targetTexture = new RenderTexture(renderResolution[0], renderResolution[1], renderTexture.depth);
        
        // Save texture from camera
        _readBackTexture = new Texture2D(renderResolution[0], renderResolution[1], TextureFormat.RGB24, false);
        
        // Debug
        Manager.singleton.ShowInfos(InfosType.captureResolution, renderResolution.x+"x"+renderResolution.y);
    }

    public void StartRecording()
    {
        _videoFrames = new List<Texture2D>();
    }

    public void Recording(IEnumerable<MP4Recorder> recorders)
    {
        cameraRenderTexture.Render();
        var ts = new FixedIntervalClock(15).timestamp;
        RenderTexture.active = cameraRenderTexture.targetTexture;
        foreach (var mp4Recorder in recorders)
        {
            var (width, height) = mp4Recorder.frameSize;
            var sample = new Texture2D(width, height, TextureFormat.RGB24, false);
            sample.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            mp4Recorder.CommitFrame(sample.GetPixels32(), ts);
        }
        RenderTexture.active = null;
        
        /* _readBackTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        
        var copyOfTexture = new Texture2D(_readBackTexture.width, _readBackTexture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(_readBackTexture, copyOfTexture);
        _videoFrames.Add(copyOfTexture); */
    }

    public IEnumerator FinishRecord(IEnumerable<MP4Recorder> recorders, FixedIntervalClock fixedIntervalClock)
    {
        inFrameCommit = true;
        yield return null;
        
        // var targetTexture = cameraRenderTexture.targetTexture;

        /*var exports = recorders.Select(mp4Recorder => new Export(mp4Recorder, targetTexture)).ToList();
        
        foreach (var frame in _videoFrames)
        {
            foreach (var export in exports)
            {
                var (width, height) = export.recorder.frameSize;
                var pixels = Array.ConvertAll(frame.GetPixels(export.x, export.y, width, height), c => (Color32) c);
                export.recorder.CommitFrame(pixels, fixedIntervalClock.timestamp);
            }
            Destroy(frame);
            yield return null;
        }*/
        
        /* foreach (var mp4Recorder in recorders)
        {
            var (width, height) = mp4Recorder.frameSize;
            
            // Calculate position to center record on texture recorded
            // ReSharper disable once PossibleLossOfFraction
            var x = Mathf.FloorToInt((targetTexture.width - width) / 2);
            var size = x + width;
            
            // Save each frame on recorder
            foreach (var frame in _videoFrames)
            {
                var pixels = frame.GetPixels32().Where((color32, i) => {
                    var pos = i % width;
                    return pos <= x && pos > size;
                }).ToArray(); // frame.GetPixels(x, y, mp4Recorder.frameSize.width, mp4Recorder.frameSize.height);
                mp4Recorder.CommitFrame(pixels, fixedIntervalClock.timestamp);
                yield return null;
            }
            
            // Debug.Log("Finish record " + mp4Recorder.frameSize);
        } */
            
        /*foreach (var videoFrame in _videoFrames)
        {
            Destroy(videoFrame);
        }*/
        
        inFrameCommit = false;
    }

    public void VideoDispose()
    {
        _videoFrames = null;
    }

    public void ValueChangeCheck(float value)
    {
        /* Vector2Int chosenRes = resolutions[(int)value];
        RenderTexture rt = new RenderTexture(chosenRes.x, chosenRes.y, renderTexture.depth);
        cameraRenderTexture.targetTexture = rt;
        videoCaptureResolution.x = rt.width;
        videoCaptureResolution.y = rt.height;
        readBackTexture = new Texture2D(videoCaptureResolution.x, videoCaptureResolution.y, TextureFormat.RGB24, false);
        Manager.singleton.ShowInfos(InfosType.captureResolution, videoCaptureResolution.x + "x" + videoCaptureResolution.y);
        Manager.singleton.ReloadRecorder();*/
    }

    /* public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    } */

    #endregion
}
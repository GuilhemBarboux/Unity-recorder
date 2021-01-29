using NatSuite.Devices;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Sharing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum InfosType
{
    cameraResolution,
    micChannels,
    backgroundResolution,
    captureResolution,
    recordingStatus
}

[Serializable]
public class ExportResolution
{
    [SerializeField]
    public string name;
    [SerializeField]
    public Vector2Int dimension;
    [HideInInspector]
    public bool active;
}

public class Manager : MonoBehaviour
{
    #region Fields

    public static Manager singleton;

    public int maxTimeRecord;
    [SerializeField]
    private Text[] infosTexts;
    [SerializeField]
    private Image fillCircle;
    [SerializeField]
    private Color colorGrey;
    [SerializeField]
    private Color colorRed;
    [SerializeField]
    private ExportResolution[] resolutions;

    private ImageRecorder imageRecorder;
    private VoiceRecorder voiceRecorder;

    private bool grantedCamera = false;
    private bool grantedMicrophone = false;

    [HideInInspector]
    public bool isSaving = false;
    private bool recording = false;
    private float timeStartRecord;
    [HideInInspector]
    public float videoDuration;

    public ICameraDevice cameraDevice;
    public IAudioDevice audioDevice;

    private Dictionary<string, MP4Recorder> _recorders = new Dictionary<string, MP4Recorder>();
    private RealtimeClock clockAudio;
    private RealtimeClock clockVideo;

    #endregion

    #region Unity's Methods

    private async void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Debug.LogError("Manager allready Instantiated");
            Destroy(gameObject);
        }

        // Initialize resolutions if not set
        resolutions ??= new ExportResolution[1];
        if (resolutions.Length == 0)
        {
            resolutions.SetValue(new ExportResolution
            {
                name = "16:9",
                dimension = new Vector2Int(1920, 1080),
                active = true
            }, 0);
        }

        // Initialize recorders
        ShowInfos(InfosType.recordingStatus, "Programme Init");
        imageRecorder = GetComponent<ImageRecorder>();
        voiceRecorder = GetComponent<VoiceRecorder>();
#if UNITY_IPHONE
        grantedCamera = await MediaDeviceQuery.RequestPermissions<ICameraDevice>();
        grantedMicrophone = await MediaDeviceQuery.RequestPermissions<IAudioDevice>();
#endif
        MediaDeviceQuery queryCamera = new MediaDeviceQuery(
            device => device is ICameraDevice
        );
        Debug.Log("super");
#if UNITY_IPHONE
        Debug.Log(queryCamera.devices.Length.ToString());
        if (queryCamera.devices.Length > 1)
        {
            Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[1];

        }
        else
        {
            Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[0];
        }
#else
        Debug.Log(queryCamera.devices.Length);
        Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[0];
#endif
        // Initialize Image Recorder
        imageRecorder.StartCameraCapture(resolutions);
        
        // Initialize Audio Recorder
        MediaDeviceQuery queryMic = new MediaDeviceQuery(
            device => device is IAudioDevice
        );
        audioDevice = (IAudioDevice)queryMic.devices[0];
        voiceRecorder.StartMicCapture();
        
        // Create MP4 recorders
        StartCoroutine(LoadRecorder());
    }

    private void Update()
    {
        if (recording && timeStartRecord + maxTimeRecord <= Time.time)
        {
            StopRecording(maxTimeRecord);
            return;
        }
        
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            if (!recording && !isSaving && _recorders != null) return;
            fillCircle.fillAmount = 1 - (Time.time - timeStartRecord) / maxTimeRecord;
        }
        else if (recording)
        {
            StopRecording(Time.time - timeStartRecord);
        }
    }

    #endregion

    #region Public Methods

    public void ShowInfos(InfosType infosType, string value)
    {
        switch (infosType)
        {
            case InfosType.cameraResolution:
                infosTexts[(int)infosType].text = "Camera Resolution : " + value;
                break;
            case InfosType.micChannels:
                infosTexts[(int)infosType].text = "Mic Channels : " + value;
                break;
            case InfosType.backgroundResolution:
                infosTexts[(int)infosType].text = "Background Resolution : " + value;
                break;
            case InfosType.captureResolution:
                infosTexts[(int)infosType].text = "Capture Resolution : " + value;
                break;
            case InfosType.recordingStatus:
                infosTexts[(int)infosType].text = "Recording Status : " + value;
                break;
        }
    }

    public void OnStartedRecord()
    {
        if (recording || isSaving) return;
        
        // Reset button and start record
        fillCircle.fillAmount = 1;
        fillCircle.color = colorRed;
        StartRecording();
    }

    #endregion

    #region Private Methods

    private async void StartRecording()
    {
        recording = true;
        timeStartRecord = Time.time;
        ShowInfos(InfosType.recordingStatus, "Started Recording");
        clockAudio = new RealtimeClock();
        clockVideo = new RealtimeClock();
        voiceRecorder.StartRecording(clockAudio);
        imageRecorder.StartRecording();
        while (recording)
        {
            imageRecorder.Recording();
            await Task.Delay(33);
        }
        clockVideo.paused = true;
    }

    private void StopRecording(float videoDuration)
    {
        ShowInfos(InfosType.recordingStatus, "Stop Record");
        this.videoDuration = videoDuration;
        isSaving = true;
        recording = false;
        // Debug.Log("Stop recording, video duration: " + videoDuration);
        
        imageRecorder.inFrameCommit = true;
        voiceRecorder.inRobotRecord = true;

        var recorders = _recorders.Values.ToArray();
        // voiceRecorder.StartRecordingChangedVoice(recorders, clockAudio);
        StartCoroutine(imageRecorder.FinishRecord(recorders, new FixedIntervalClock(15)));
        StartCoroutine(Recording());
    }

    private IEnumerator Recording()
    {
        ShowInfos(InfosType.recordingStatus, "Saving Texture and Robot Voice");
        yield return new WaitUntil(() => !imageRecorder.inFrameCommit && !voiceRecorder.inRobotRecord);
        SaveVideo();
    }

    private async void SaveVideo()
    {
        ShowInfos(InfosType.recordingStatus, "Saving Video");

        // Save paths
        var paths = new Dictionary<string, string>();
        foreach (var r in _recorders)
        {
            paths[r.Key] = await r.Value.FinishWriting();
        }
        Debug.Log(paths.ToString());
        
        // Share medias files on sharing system
#if UNITY_IPHONE
        var sp = new SharePayload();
        foreach (var path in paths)
        {
            await sp.AddMedia(path.Value).Commit();
        }
#endif
        
        DisposeVideo();
    }

    private void DisposeVideo()
    {
        ShowInfos(InfosType.recordingStatus, "Disposing");
        voiceRecorder.AudioDispose();
        imageRecorder.VideoDispose();
        CreateRecorder();
        isSaving = false;
        fillCircle.fillAmount = 1;
        fillCircle.color = colorGrey;
        ShowInfos(InfosType.recordingStatus, "Ready For Capture");
    }

    private IEnumerator LoadRecorder()
    {
        Debug.Log("before wait");
        yield return new WaitUntil(() => voiceRecorder.micChannels != -1 && imageRecorder.videoCaptureResolution.x != -1);
        Debug.Log("after wait");
        CreateRecorder();
        ShowInfos(InfosType.recordingStatus, "Ready For Capture");
    }
    
    private void CreateRecorder()
    {
        _recorders.Clear();
        
        foreach (var exportResolution in resolutions)
        {
            if (exportResolution.active)
            {
                _recorders.Add(exportResolution.name, new MP4Recorder(exportResolution.dimension.x, exportResolution.dimension.y, 30, 48000, audioDevice.channelCount));
            }
        }
    }

    #endregion
}

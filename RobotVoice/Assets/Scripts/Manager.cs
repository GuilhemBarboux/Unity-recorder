using NatSuite.Devices;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Sharing;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum InfosType
{
    cameraResolution,
    micChannels,
    backgroundResolution,
    captureResolution,
    recordingStatus
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

    private MP4Recorder recorder;
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
#if UNITY_IPHONE
        if (queryCamera.devices.Length > 1)
        {
            Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[1];

        }
        else
        {
            Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[0];
        }
#else
        Manager.singleton.cameraDevice = (ICameraDevice)queryCamera.devices[0];
#endif
        imageRecorder.StartCameraCapture();
        MediaDeviceQuery queryMic = new MediaDeviceQuery(
            device => device is IAudioDevice
        );
        audioDevice = (IAudioDevice)queryMic.devices[0];
        voiceRecorder.StartMicCapture();
        StartCoroutine(CreateRecorder());
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
            if (!recording && !isSaving && recorder != null)
            {
                /*fillCircle.fillAmount = 1;
                fillCircle.color = colorRed;
                StartRecording();*/
                return;
            }
            else
            {
                fillCircle.fillAmount = 1-(Time.time-timeStartRecord)/maxTimeRecord;
                return;
            }
        }
        else if (recording)
        {
            StopRecording(Time.time - timeStartRecord);
            return;
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

    public void ReloadRecorder()
    {
        recorder = new MP4Recorder(imageRecorder.videoCaptureResolution.x, imageRecorder.videoCaptureResolution.y, 30, 48000, audioDevice.channelCount);
    }

    public void OnStartedRecord()
    {
        if (!recording && !isSaving)
        {
            fillCircle.fillAmount = 1;
            fillCircle.color = colorRed;
            StartRecording();
        }
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
        Debug.Log("Stop recording, video duration: " + videoDuration);
        imageRecorder.inFrameCommit = true;
        voiceRecorder.inRobotRecord = true;
        voiceRecorder.StartRecordingChangedVoice(recorder, clockAudio);
        StartCoroutine(imageRecorder.FinishRecord(recorder, new FixedIntervalClock(15)));
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
        string path = await recorder.FinishWriting();
        Debug.Log(path);
#if UNITY_EDITOR
#elif UNITY_IPHONE
        SharePayload sp = new SharePayload();
        await sp.AddMedia(path).Commit();
#endif
        DisposeVideo();
    }

    private void DisposeVideo()
    {
        ShowInfos(InfosType.recordingStatus, "Disposing");
        voiceRecorder.AudioDispose();
        imageRecorder.VideoDispose();
        recorder = new MP4Recorder(imageRecorder.videoCaptureResolution.x, imageRecorder.videoCaptureResolution.y, 30, 48000, audioDevice.channelCount);
        isSaving = false;
        fillCircle.fillAmount = 1;
        fillCircle.color = colorGrey;
        ShowInfos(InfosType.recordingStatus, "Ready For Capture");
    }

    private IEnumerator CreateRecorder()
    {
        yield return new WaitUntil(() => voiceRecorder.micChannels != -1 && imageRecorder.videoCaptureResolution.x != -1);
        recorder = new MP4Recorder(imageRecorder.videoCaptureResolution.x, imageRecorder.videoCaptureResolution.y, 30, 48000, audioDevice.channelCount);
        ShowInfos(InfosType.recordingStatus, "Ready For Capture");
    }

    #endregion
}

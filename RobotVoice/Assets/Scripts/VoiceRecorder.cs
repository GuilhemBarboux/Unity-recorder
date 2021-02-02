using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Sharing;
using UnityEngine;

public class VoiceRecorder : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioListener audioListener;

    [HideInInspector]
    public bool inRobotRecord;

    private List<AudioInput> audioInputs;

    [HideInInspector]
    public int micChannels;

    #endregion

    #region Public Methods

    public void StartMicCapture()
    {
        StartCoroutine(TakeMicrophoneInfos());
    }

    public void StartRecording(RealtimeClock clock)
    {
        clock.paused = true;
        audioSource.clip = Microphone.Start("", true, Manager.singleton.maxTimeRecord, 44100);
    }

    public void StartRecordingChangedVoice(MP4Recorder[] recorders, RealtimeClock clock)
    {
        Microphone.End("");
        
        clock.paused = false;
        var clock2 = new RealtimeClock();
        
        audioInputs = new List<AudioInput>(recorders.Length);
        
        foreach (var mp4Recorder in recorders)
        {
            audioInputs.Add(new AudioInput(mp4Recorder, clock2, audioListener));
        }

        audioSource.Play();

        FinishRecordingChangedVoice(Manager.singleton.videoDuration);
    }

    private async void FinishRecordingChangedVoice(float timeAudio)
    {
        await Task.Delay((int)(timeAudio * 1000)+500);
        audioInputs.ForEach(audioInput => audioInput.Dispose());
        audioSource.Stop();
        await Task.Delay(500);
        inRobotRecord = false;
    }

    public void AudioDispose()
    {
        Destroy(audioSource.clip);
    }

    #endregion

    #region Private Methods

    private IEnumerator TakeMicrophoneInfos()
    {
        AudioClip tmpClip = Microphone.Start("", false, 1, 44100);
        yield return new WaitUntil(() => tmpClip != null);
        micChannels = tmpClip.channels;
        Manager.singleton.ShowInfos(InfosType.micChannels, micChannels.ToString());
        Destroy(tmpClip);
    }

    #endregion
}
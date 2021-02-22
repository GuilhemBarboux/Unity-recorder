using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Record
{
    [RequireComponent(typeof(AudioListener))]
    public class MediaRecorder : MonoBehaviour
    {
        public const int MAXDurationS = 60;

        [SerializeField] private Camera renderCamera;
        [SerializeField] private GameObject button;
        [SerializeField] private AudioSource microphoneSource;
        [SerializeField] private Image background;
        private MediaExport[] exports = new MediaExport[0];
        private List<CameraInput> cameraInputs; // Camera input for recording video
        private List<MP4Recorder> recorders; // Recorder that will record an MP4
        public UnityEvent<string[]> onFinishRecord;

        private bool recording; // Recording video
        private bool ready; // Ready to new record
        private IClock clock;
        private IClock AudioClock;
        private float timeStart;

        // Hack to set sample rate on each microphone record (IOS) 
        [DllImport ("__Internal")]
        private static extern void SetPreferredSampleRate(int sampleRate);
        private static void EnableRecording()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
        SetPreferredSampleRate(AudioSettings.outputSampleRate);
#endif
        }

        private void Awake()
        {
            recorders = new List<MP4Recorder>();
            cameraInputs = new List<CameraInput>();

            if (microphoneSource == null) microphoneSource = new AudioSource();

            microphoneSource.mute = false;
            microphoneSource.loop = false;
            microphoneSource.bypassEffects = false;
            microphoneSource.bypassListenerEffects = false;
        }

        private IEnumerator Start()
        {
            // Start microphone to avoid freeze
            microphoneSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            Microphone.End(null);

            // Activate button
            button.SetActive(true);

            // App ready to start record
            ready = true;
        }

        private void OnDestroy()
        {
            // Stop microphone
            if (microphoneSource != null) microphoneSource.Stop();
            Microphone.End(null);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (recording)
                // Save audio sample on all recorders
                try
                {
                    foreach (var mp4Recorder in recorders) mp4Recorder.CommitSamples(data, AudioClock.timestamp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            // Mute sound to avoid voice return
            // TODO : on play video, don't mute
            Array.Clear(data, 0, data.Length);
        }

        private IEnumerator StartMicrophone()
        {
            microphoneSource.clip = Microphone.Start(null, true, 60, AudioSettings.outputSampleRate);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            AudioClock = new RealtimeClock();
            microphoneSource.Play();
            recording = true;
        }

        public void StartRecording()
        {
            if (!ready) return;
            ready = false;

            // Create the MP4 recorder
            CreateRecorder();

            // Start microphone
            StartCoroutine(StartMicrophone());
            
            // Save timestart to replay video time
            timeStart = Time.time;

            // Create audio and camera input
            clock = new RealtimeClock();
            foreach (var mp4Recorder in recorders) cameraInputs.Add(new CameraInput(mp4Recorder, clock, renderCamera));
        }

        public async void StopRecording()
        {
            // Stop recording
            recording = false;
            var duration = (Time.time - timeStart) * 1000;

            // Stop streaming media to the recorder
            cameraInputs.ForEach(ci => ci.Dispose());
            AudioClock = null;

            // Stop Microphone
            await Task.Run(() => new WaitUntil(() => AudioClock.timestamp >= clock.timestamp));
            Microphone.End(null);
            microphoneSource.clip = null;
            clock = null;

            // Finish writing video
            var paths = new string[0];
            try
            {
                paths = await Task.WhenAll(recorders.Select(item => item.FinishWriting()).ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // Clear List
            recorders.Clear();
            cameraInputs.Clear();

            await Task.Run(() => new WaitForSeconds(2f));
            // Share medias
            if (paths.Length > 0)
            {
                await Task.Run(() => new WaitForEndOfFrame());
#if UNITY_EDITOR
                Debug.Log("Record video of duration " + duration + "ms");
#endif
                onFinishRecord.Invoke(paths);
            }

            // Reset recorders
            ready = true;
        }

        private void CreateRecorder()
        {
            foreach (var export in exports)
                recorders.Add(new MP4Recorder(export.dimension.x, export.dimension.y, 30,
                    AudioSettings.outputSampleRate, (int) AudioSettings.speakerMode));
        }

        public void SetDimensions(MediaExport[] mediaExports)
        {
            exports = mediaExports;
        }

        public void SetBackground(Material mediaBackground)
        {
            if (mediaBackground) background.material = mediaBackground;
        }
    }
}
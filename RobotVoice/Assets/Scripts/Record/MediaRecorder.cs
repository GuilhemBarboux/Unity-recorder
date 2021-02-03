using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatSuite.Devices;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Sharing;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Record
{
    [Serializable]
    public class MediaExport
    {
        [SerializeField] public bool active = true;
        [SerializeField] public Vector2Int dimension;
    }

    [RequireComponent(typeof(AudioListener))]
    public class MediaRecorder : MonoBehaviour
    {
        [SerializeField] private AudioSource microphoneSource;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private GameObject button;
        [SerializeField] private MediaExport[] exports;
        // [SerializeField] private int maxDuration = 300;
        private List<AudioInput> audioInputs; // Audio input for recording video
        private List<CameraInput> cameraInputs; // Camera input for recording video
        private List<MP4Recorder> recorders; // Recorder that will record an MP4
        
        private bool recording; // Recording video
        private bool ready; // Ready to new record

        private IClock clock;

        private void Awake()
        {
            recorders = new List<MP4Recorder>();
            cameraInputs = new List<CameraInput>();


            if (microphoneSource == null)
            {
                microphoneSource = new AudioSource {loop = true};
            }
        }

        private IEnumerator Start()
        {
            // Enable and disable microphone to avoid lag on ios
            microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            Microphone.End(null);
            
            // Activate button
            button.SetActive(true);
        }
        private void Update()
        {
            if (Microphone.GetPosition(null) > 0 && !microphoneSource.isPlaying)
            {
                microphoneSource.Play();
            }
        }

        private void OnDestroy () {
            // Stop microphone
            microphoneSource.Stop();
            Microphone.End(null);
        }

        private void OnAudioFilterRead (float[] data, int channels)
        {
            if (!recording) return;
            
            foreach (var mp4Recorder in recorders)
            {
                mp4Recorder.CommitSamples(data, clock.timestamp);
            }
            
            Array.Clear(data, 0, data.Length);
        }

        public void StartRecording()
        {
            if (ready) return;
            recording = true;
            ready = false;
            
            // Start microphone
            microphoneSource.clip = Microphone.Start(null, true, 300, AudioSettings.outputSampleRate);
            
            // Create the MP4 recorder
            clock = new RealtimeClock();
            CreateRecorder();
            
            // Create audio and camera input
            foreach (var mp4Recorder in recorders)
            {
                cameraInputs.Add(new CameraInput(mp4Recorder, clock, renderCamera));
            }
        }

        public async void StopRecording()
        {  
            // Stop microphone
            recording = false;
            Microphone.End(null);
            
            // Stop streaming media to the recorder
            cameraInputs.ForEach(ci => ci.Dispose());
            
            // Finish writing video
            var paths = await Task.WhenAll(recorders.Select(item => item.FinishWriting()).ToList());

            // Share medias
#if UNITY_IPHONE && !UNITY_EDITOR
            var sp = new SharePayload();
            foreach (var path in paths) sp.AddMedia(path);
            await sp.Commit();
#else            
            foreach (var path in paths) Debug.Log(path);
#endif
            // Clear List
            cameraInputs.Clear();
            
            // Reset recorders
            CreateRecorder();
            ready = true;
        }

        private void CreateRecorder()
        {
            recorders.Clear();
            foreach (var export in exports)
            {
                if (!export.active) continue;
                recorders.Add(new MP4Recorder(export.dimension.x, export.dimension.y, 30, AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode));
            }
        }
    }
}
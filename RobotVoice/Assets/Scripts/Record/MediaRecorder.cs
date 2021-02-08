using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NatSuite.Devices;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Sharing;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

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
        [SerializeField] private int latencyMS = 1000;
        private List<CameraInput> cameraInputs; // Camera input for recording video
        private List<MP4Recorder> recorders; // Recorder that will record an MP4
        
        private bool recording; // Recording video
        private bool ready; // Ready to new record

        private IClock clock;
        private float timeStart;

        private void Awake()
        {
            recorders = new List<MP4Recorder>();
            cameraInputs = new List<CameraInput>();

            if (microphoneSource == null) microphoneSource = new AudioSource();
            
            microphoneSource.mute = true;
            microphoneSource.loop = true;
            microphoneSource.bypassEffects = false;
            microphoneSource.bypassListenerEffects = false;
        }

        private IEnumerator Start()
        {
            // Start record microphone in a loop
            microphoneSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            microphoneSource.Play();
            
            // Activate button
            button.SetActive(true);
            
            // App ready to start record
            ready = true;
        }

        private void OnDestroy () {
            // Stop microphone
            if (microphoneSource != null) microphoneSource.Stop();
            Microphone.End(null);
        }

        private void OnAudioFilterRead (float[] data, int channels)
        {
            if (recording)
            {
                // Save audio sample on all recorders
                try
                {
                    foreach (var mp4Recorder in recorders)
                    {
                        mp4Recorder.CommitSamples(data, clock.timestamp);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
            // Mute sound to avoid voice return
            // TODO : on play video, don't mute
            Array.Clear(data, 0, data.Length);
        }

        public void StartRecording()
        {
            if (!ready) return;
            ready = false;
            
            // Start microphone
            microphoneSource.mute = false;
            
            // Create the MP4 recorder
            CreateRecorder();
            
            // Save timestart to replay video time
            timeStart = Time.time;
            
            // Create audio and camera input
            clock = new RealtimeClock();
            foreach (var mp4Recorder in recorders)
            {
                cameraInputs.Add(new CameraInput(mp4Recorder, clock, renderCamera));
            }
            
            // Start recording
            recording = true;
        }

        public async void StopRecording()
        {  
            // Stop recording
            recording = false;
            var duration = (Time.time - timeStart) * 1000;
            
            // Stop Microphone
            microphoneSource.mute = true;
            
            // Wait finish rendering scene
            await Task.Run(() => new WaitForEndOfFrame());
            
            // Stop streaming media to the recorder
            cameraInputs.ForEach(ci => ci.Dispose());
            
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

            // Share medias
            if (paths.Length > 0)
            {
#if UNITY_IPHONE && !UNITY_EDITOR
                Handheld.PlayFullScreenMovie($"file://{paths[0]}");
                var sp = new SharePayload();
                foreach (var path in paths) sp.AddMedia(path);
                await Task.Delay((int)duration);
                await sp.Commit();
#else
                Debug.Log("Duration " + duration);
                foreach (var path in paths) Debug.Log(path);
#endif
            }
            else
            {
                // TODO : display Error
            }
            
            // Reset recorders
            ready = true;
        }

        private void CreateRecorder()
        {
            foreach (var export in exports)
            {
                if (!export.active) continue;
                recorders.Add(new MP4Recorder(export.dimension.x, export.dimension.y, 30, AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode));
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

namespace Record
{
    [RequireComponent(typeof(AudioListener))]
    public class MediaRecorder : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera;
        [SerializeField] private GameObject button;
        [SerializeField] private AudioSource microphoneSource;
        [SerializeField] private Image background;
        private MediaExport[] exports;
        private List<CameraInput> cameraInputs; // Camera input for recording video
        private List<MP4Recorder> recorders; // Recorder that will record an MP4
        
        private bool recording; // Recording video
        private bool ready; // Ready to new record

        private IClock clock;
        private IClock AudioClock;
        private float timeStart;
        private Texture2D readbackBuffer;
        private byte[] pixelBuffer;
        private RenderTextureDescriptor frameDescriptor;

        private void Awake()
        {
            recorders = new List<MP4Recorder>();
            cameraInputs = new List<CameraInput>();

            if (microphoneSource == null) microphoneSource = new AudioSource();
            
            microphoneSource.mute = false;
            microphoneSource.loop = false;
            microphoneSource.bypassEffects = false;
            microphoneSource.bypassListenerEffects = false;
            
            readbackBuffer = new Texture2D(1920, 1080, TextureFormat.RGBA32, false, false);
            pixelBuffer = new byte[1920 * 1080 * 4];
            frameDescriptor = new RenderTextureDescriptor(1920, 1080, RenderTextureFormat.ARGB32, 24);
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

        private void OnDestroy () {
            // Stop microphone
            if (microphoneSource != null) microphoneSource.Stop();
            Microphone.End(null);
        }

        private void Update()
        {
            if (Microphone.IsRecording(null) && Microphone.GetPosition(null) > 0 && !microphoneSource.isPlaying)
            {
                AudioClock = new RealtimeClock();
                microphoneSource.Play();
                recording = true;
            }
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
                        mp4Recorder.CommitSamples(data, AudioClock.timestamp);
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

        /* private void EndFrameRendering(ScriptableRenderContext scriptableRenderContext, Camera[] cameras)
        {
            Debug.Log("EndFrameRendering");
            if (clock == null) return;

            RenderTexture.active = renderCamera.targetTexture;
            
            
            var frameBuffer = RenderTexture.GetTemporary(frameDescriptor);
            var prevTarget = renderCamera.targetTexture;
            renderCamera.targetTexture = frameBuffer;
            renderCamera.Render();
            renderCamera.targetTexture = prevTarget;
            var timestamp = clock.timestamp;
            var prevActive = RenderTexture.active;
            RenderTexture.active = frameBuffer;
            readbackBuffer.ReadPixels(new Rect(0, 0, frameBuffer.width, frameBuffer.height), 0, 0, false);
            readbackBuffer.GetRawTextureData<byte>().CopyTo(pixelBuffer);
            foreach (var mp4Recorder in recorders)
            {
                mp4Recorder.CommitFrame(pixelBuffer, timestamp);
            }
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(frameBuffer);
        } */

        public void StartRecording()
        {
            if (!ready) return;
            ready = false;
            
            // Create the MP4 recorder
            CreateRecorder();
            
            // Start microphone
            EnableRecording();
            microphoneSource.clip = Microphone.Start(null, true, 60, AudioSettings.outputSampleRate);
            
            // Save timestart to replay video time
            timeStart = Time.time;
            
            // Create audio and camera input
            clock = new RealtimeClock();
            foreach (var mp4Recorder in recorders)
            {
                cameraInputs.Add(new CameraInput(mp4Recorder, clock, renderCamera));
            }
        }

        public async void StopRecording()
        {  
            // Stop recording
            recording = false;
            var duration = (Time.time - timeStart) * 1000;
            
            // Stop Microphone
            Microphone.End(null);

            // Wait finish rendering scene
            await Task.Run(() => new WaitForEndOfFrame());
            
            // Stop streaming media to the recorder
            cameraInputs.ForEach(ci => ci.Dispose());
            clock = null;
            AudioClock = null;
            
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
#if UNITY_IPHONE && !UNITY_EDITOR
                // Handheld.PlayFullScreenMovie($"file://{paths[0]}");
                microphoneSource.Play();
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
            microphoneSource.clip = null;
            await Task.Run(() => new WaitForSeconds(1f));
            ready = true;
        }

        private void CreateRecorder()
        {
            foreach (var export in exports)
            {
                Debug.Log("Create recorder " + export.dimension.x + "x" + export.dimension.y);
                recorders.Add(new MP4Recorder(export.dimension.x, export.dimension.y, 30, AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode));
            }
        }

        public void SetDimensions(MediaExport[] mediaExports)
        {
            exports = mediaExports;
        }

        public void SetBackground(Material mediaBackground)
        {
            if (mediaBackground)
            {
                background.material = mediaBackground;
            }
        } 

#if UNITY_IPHONE && !UNITY_EDITOR
        
        [DllImport ("__Internal")]
        private static extern void SetPreferredSampleRate(int sampleRate);
#endif
        private static void EnableRecording() {
#if UNITY_IPHONE && !UNITY_EDITOR
        SetPreferredSampleRate(AudioSettings.outputSampleRate);
#endif
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatSuite.Recorders.Clocks;
using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NatSuite.Sharing;
using Nrjwolf.Tools;
using TMPro;
using UnityEngine.Video;

namespace UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private RatioButton[] ratios;
        [SerializeField] private Animator[] panels;
        [SerializeField] private Animator[] buttons;
        [SerializeField] private string triggerPanelName;
        [SerializeField] private string triggerButtonName;
        [SerializeField] private string triggerButtonSelectedName;
        [SerializeField] private GameObject[] menuActions;
        [SerializeField] private GameObject[] recordActions;
        [SerializeField] private GameObject[] replayActions;
        [SerializeField] private GameObject recordButton;
        [SerializeField] private Animator replayAnimator;
        [SerializeField] private TextMeshProUGUI recordTimer;
        [SerializeField] private TextMeshProUGUI restRecordTimer;
        [SerializeField] private VideoPlayer videoPlayer;
        
        public UnityEvent<MediaExport[]> onRatiosChanged;
        public UnityEvent<Material> onBackgroundChanged;

        private bool isTransition;
        private int transitionDuration = 800;
        private IClock clock;
        private bool recording;
        private float recordDuration;
        private BackgroundButton[] backgroundButtons;
        private string[] paths = new string[0];
        private static readonly int Replay = Animator.StringToHash("replay");

        private static string GetTimer(long time)
        {
            var stamp = Mathf.Round(time);
            var seconds = stamp % 60;
            var minutes = Mathf.Floor(stamp / 60);
            return $"{minutes:00}:{seconds:00}";
        }

        private string duration => GetTimer(recording ? clock.timestamp / 1000000000 : 0);

        private string reverseDuration => GetTimer(recording ? MediaRecorder.MAXDurationS - clock.timestamp / 1000000000 : 0);

        private void Awake()
        {
            backgroundButtons = GetComponentsInChildren<BackgroundButton>();

            foreach (var backgroundButton in backgroundButtons)
            {
                backgroundButton.onValueChanged.AddListener(SendBackground);
            }
        }

        private void Start()
        {
            SendRatios();
            
            foreach (var ratio in ratios)
            {
                ratio.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
            }

            if (backgroundButtons.Length > 0)
            {
                backgroundButtons[0].GetComponent<Toggle>().isOn = true;
            }

            videoPlayer.enabled = false;
            videoPlayer.started += OnVideoReplayLoop;
            videoPlayer.loopPointReached += OnVideoReplayLoop;
        }

        private void Update()
        {
            if (!recording) return;
            recordTimer.text = duration;
            restRecordTimer.text = reverseDuration;
        }

        private async Task CloseAll()
        {
            var count = panels.Count(panel => panel.GetBool(triggerPanelName));
            if (count <= 0) return;
            
            foreach (var button in buttons)
            {
                button.SetBool(triggerButtonName, false);
            }
            foreach (var panel in panels)
            {
                panel.SetBool(triggerPanelName, false);
            }
            await Task.Delay(transitionDuration);
        }

        public async void Close()
        {
            if (isTransition) return;
            isTransition = true;
            await CloseAll();
            isTransition = false;
        }

        private async void Open(Animator panel)
        {
            if (isTransition) return;
            isTransition = true;
            await CloseAll();
            panel.SetBool(triggerPanelName, true);
            foreach (var button in buttons)
            {
                button.SetBool(triggerButtonName, true);
            }
            await Task.Delay(transitionDuration);
            isTransition = false;
        }

        public void Selected(Animator selected)
        {
            selected.SetBool(triggerButtonSelectedName, true);
        }

        public void Toggle(Animator panel)
        {
            foreach (var button in buttons)
            {
                button.SetBool(triggerButtonSelectedName, false);
            }
            
            if (panel.GetBool(triggerPanelName))
            {
                Close();
            }
            else
            {
                Open(panel);
            }
        }

        private void OnValueChanged(bool value)
        {
            var toggles = ratios.Select(item => item.GetComponent<Toggle>()).ToList();
            var wasOnCount = toggles.Count(toggle => toggle.isOn);

            if (wasOnCount == 1)
            {
                foreach (var toggle in toggles.Where(t => t.isOn))
                {
                    toggle.enabled = false;
                }
            }
            else
            {
                foreach (var toggle in toggles)
                {
                    toggle.enabled = true;
                }
            }

            SendRatios();
        }

        private void HideAllMenu()
        {
            foreach (var action in menuActions)
            {
                action.SetActive(false);
            }
            foreach (var action in recordActions)
            {
                action.SetActive(false);
            }
            foreach (var action in replayActions)
            {
                action.SetActive(false);
            }
        }

        public async void OnStartRecord()
        {
            clock = new RealtimeClock();
            recording = true;
            HideAllMenu();
            foreach (var action in recordActions)
            {
                action.SetActive(true);
            }
            await CloseAll();
        }

        public void OnStopRecord()
        {
            recordDuration = clock.timestamp;
            recording = false;
            clock = null;
        }

        private void OnRestartRecord()
        {
            HideAllMenu();
            foreach (var action in menuActions)
            {
                action.SetActive(true);
            }
            videoPlayer.Pause();
            videoPlayer.enabled = false;
            recordButton.SetActive(true);
            replayAnimator.gameObject.SetActive(false);
        }

        public void Restart()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            IOSNativeAlert.ShowSheetMessage(
                "Restart",
                "Are you sure you want to start over ? This video will be deleted.",
                new IOSNativeAlert.AlertButton("Keep", null, ButtonStyle.Cancel),
                new IOSNativeAlert.AlertButton("Restart", OnRestartRecord, ButtonStyle.Destructive)
            );
#else
            OnRestartRecord();
#endif
        }

        public async void Share()
        {
            if (paths.Length <= 0) return;
            var sp = new SharePayload();
            foreach (var path in paths) sp.AddMedia(path);
            await sp.Commit();
        }

        public void OnFinishRecord(string[] mediaPaths)
        {
            paths = mediaPaths;
#if  UNITY_EDITOR
            foreach (var path in paths) Debug.Log(path);
#endif
            HideAllMenu();
            recordButton.SetActive(false);
            replayAnimator.gameObject.SetActive(true);
            foreach (var action in replayActions)
            {
                action.SetActive(true);
            }

            if (paths.Length <= 0) return;
            videoPlayer.enabled = true;
            videoPlayer.url = paths[0];
            videoPlayer.Prepare();
            videoPlayer.Play();
        }

        private void SendRatios()
        {
            onRatiosChanged.Invoke(ratios.Where(i => i.GetComponent<Toggle>().isOn).Select(i => i.dimension).ToArray());
        }

        private void SendBackground(Material background)
        {
            onBackgroundChanged.Invoke(background);
        }

        private void OnVideoReplayLoop(VideoPlayer source)
        {
            replayAnimator.speed = 1f / (float) source.length;
            replayAnimator.SetTrigger(Replay);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NatSuite.Sharing;

namespace UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Ratio[] ratios;
        [SerializeField] private Animator[] panels;
        [SerializeField] private Animator[] buttons;
        [SerializeField] private string triggerPanelName;
        [SerializeField] private string triggerButtonName;
        [SerializeField] private GameObject[] menuActions;
        [SerializeField] private GameObject[] recordActions;
        [SerializeField] private GameObject[] replayActions;
        
        public UnityEvent<MediaExport[]> onRatiosChanged;
        public UnityEvent<Material> onBackgroundChanged;

        private bool isTransition;
        private int transitionDuration = 800;
        private float recordStartTime;
        private float recordDuration;
        private ButtonBackground[] backgroundButtons;
        private string[] paths = new string[0];

        public string GetTimer(float time)
        {
            var stamp = Mathf.Round(time);
            var seconds = stamp % 60;
            var minutes = Mathf.Floor(stamp / 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        public string Duration
        {
            get => GetTimer(Time.time - recordStartTime);
        }
        
        public string ReverseDuration
        {
            get => GetTimer(MediaRecorder.MAXDurationS - (Time.time - recordStartTime));
        }
        
        private void Awake()
        {
            backgroundButtons = GetComponentsInChildren<ButtonBackground>();

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
        }

        private void FixedUpdate()
        {
            if (recordStartTime > 0)
            {
                Debug.Log(Duration);
                Debug.Log(ReverseDuration);
            }
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

        public async void Open(Animator panel)
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

        public void Toggle(Animator panel)
        {
            if (panel.GetBool(triggerPanelName))
            {
                Close();
            }
            else
            {
                Open(panel);
            }
        }
        
        public void OnValueChanged(bool value)
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

        public void OnStartRecord()
        {
            recordStartTime = Time.time;
            HideAllMenu();
            foreach (var action in recordActions)
            {
                action.SetActive(true);
            }
        }

        public void OnStopRecord()
        {
            recordDuration = (Time.time - recordStartTime) * 1000;
            recordStartTime = 0f;
            HideAllMenu();
            foreach (var action in replayActions)
            {
                action.SetActive(true);
            }
        }

        public void Restart()
        {
            HideAllMenu();
            foreach (var action in menuActions)
            {
                action.SetActive(true);
            }
        }

        public void Replay()
        {
            if (paths.Length > 0) Handheld.PlayFullScreenMovie($"file://{paths[0]}");
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
        }

        private void SendRatios()
        {
            onRatiosChanged.Invoke(ratios.Where(i => i.GetComponent<Toggle>().isOn).Select(i => i.dimension).ToArray());
        }

        private void SendBackground(Material background)
        {
            onBackgroundChanged.Invoke(background);
        }
    }
}

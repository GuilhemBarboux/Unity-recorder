using System;
using System.Linq;
using System.Threading.Tasks;
using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Ratio[] ratios;
        [SerializeField] private Animator[] panels;
        [SerializeField] private Animator[] buttons;
        private ButtonBackground[] backgroundButtons;
        [SerializeField] private string triggerPanelName;
        [SerializeField] private string triggerButtonName;

        private bool isTransition;
        private int duration = 800;
        
        public UnityEvent<MediaExport[]> OnRatiosChanged;
        public UnityEvent<Material> OnBackgroundChanged;

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
            foreach (var ratio in ratios)
            {
                ratio.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
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
            await Task.Delay(duration);
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
            await Task.Delay(duration);
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

        private void SendRatios()
        {
            OnRatiosChanged.Invoke(ratios.Where(i => i.GetComponent<Toggle>().isOn).Select(i => i.dimension).ToArray());
        }

        private void SendBackground(Material background)
        {
            OnBackgroundChanged.Invoke(background);
        }
    }
}

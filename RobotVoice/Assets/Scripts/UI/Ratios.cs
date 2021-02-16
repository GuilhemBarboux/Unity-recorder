using System;
using System.Linq;
using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class Ratios : MonoBehaviour
    {
        public UnityEvent<MediaExport[]> OnRatiosChanged;
        private Ratio[] items;

        private void Awake()
        {
            items = GetComponentsInChildren<Ratio>();
            SendRatios();
        }

        public void OnValueChanged(bool value)
        {
            var toggles = items.Select(item => item.GetComponent<Toggle>()).ToList();
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
            OnRatiosChanged.Invoke(items.Where(i => i.GetComponent<Toggle>().isOn).Select(i => i.dimension).ToArray());
        }
    }
}

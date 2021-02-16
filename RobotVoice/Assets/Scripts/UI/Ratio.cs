using System;
using Record;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Toggle)), RequireComponent(typeof(Image))]
    public class Ratio : MonoBehaviour
    {
        public MediaExport dimension;
        public Color enabledColor;
        public Color disabledColor;
        public Sprite enableMaterial;
        public Sprite disabledMaterial;
        public Image check;
        public Text title;
        public Image preview;
        private Image i;

        private void Awake()
        {
            i = GetComponent<Image>();
        }

        private void Start()
        {
            OnValueChanged(GetComponent<Toggle>().isOn);
        }

        public void OnValueChanged(bool value)
        {
            i.color = value ? enabledColor : disabledColor;
            check.color = i.color;
            check.sprite = value ? enableMaterial : disabledMaterial;
            title.color = i.color;
            preview.color = new Color(255, 255, 255, i.color.a);
        }
    }
}
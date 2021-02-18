using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Animator))]
    public class ButtonBackground : MonoBehaviour
    {
        [SerializeField] private Material background;
        public UnityEvent<Material> onValueChanged;
        private static readonly int Selected = Animator.StringToHash("selected");

        public void OnSelect(bool selected)
        {
            if (selected)
            {
                GetComponent<Animator>().SetBool(Selected, true);
                onValueChanged.Invoke(background);
            }
            else
            {
                GetComponent<Animator>().SetBool(Selected, false);
            }
        }
    }
}
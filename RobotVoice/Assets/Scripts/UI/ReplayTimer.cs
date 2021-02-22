using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ReplayTimer : MonoBehaviour
    {
        public Image countdown;

        public void SetCountdown(float value)
        {
            countdown.fillAmount = value;
        }
    }
}

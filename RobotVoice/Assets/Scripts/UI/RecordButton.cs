using System.Collections;
using NatSuite.Recorders.Clocks;
using Record;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {

	[RequireComponent(typeof(EventTrigger))]
	public class RecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

		public Image button, countdown, circle, square;
		public UnityEvent onTouchDown, onTouchUp;
		private bool pressed;
		private const float MaxRecordingTime = MediaRecorder.MAXDurationS; // seconds
		private IClock clock;

		private void Start () {
			Reset();
		}

		private void Reset () {
			// Reset fill amounts
			if (button)
				button.fillAmount = 1.0f;
			if (countdown)
				countdown.fillAmount = 0.0f;
		}

		void IPointerDownHandler.OnPointerDown (PointerEventData eventData) {
			// Start counting
			if (!pressed) StartCoroutine(Countdown());
		}

		void IPointerUpHandler.OnPointerUp (PointerEventData eventData) {
			// Reset pressed
			pressed = false;
		}

		private IEnumerator Countdown () {
			pressed = true;
			// First wait a short time to make sure it's not a tap
			yield return new WaitForSeconds(0.2f);
			if (!pressed) pressed = true; // Was a tapped
			
			// Start recording
			onTouchDown?.Invoke();
			clock = new RealtimeClock();
			
			// Animate the countdown
			circle.gameObject.SetActive(false);
			square.gameObject.SetActive(true);
			var ratio = 0f;
			const float maxTimestamp = MaxRecordingTime * 1000000000;
			while (pressed && (ratio = clock.timestamp / maxTimestamp) < 1.0f) {
				countdown.fillAmount = ratio;
				button.fillAmount = 1f - ratio;
				yield return null;
			}
			
			// Reset
			Reset();
			
			// Stop recording
			onTouchUp?.Invoke();
			circle.gameObject.SetActive(true);
			square.gameObject.SetActive(false);
		}
	}
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Animator))]
    public class ButtonToggle : MonoBehaviour
    {
        private Animator animator;
        public string triggerName = "toggle";
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Toggle()
        {
            animator.SetBool(triggerName, !animator.GetBool(triggerName));
        }
    }
}

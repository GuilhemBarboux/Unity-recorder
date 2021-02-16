using System;
using UnityEngine;

namespace UI
{
    
    [RequireComponent(typeof(Animator))]
    public class PanelToggle : MonoBehaviour
    {
        public string triggerName;
        private bool isOpen;
        private Animator animator;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            isOpen = animator.GetBool(triggerName);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            animator.SetBool(triggerName, isOpen);
        }

        public void Close()
        {
            // TODO: wait for closing to record
            animator.SetBool(triggerName, false);
        }
    }
}

using System;
using ff.utils;
using UnityEngine;
using UnityEngine.Events;

namespace ff.ar_rh_spurlab.GrayScaler
{
    public class Portal : MonoBehaviour
    {
        public delegate void PortalTriggeredDelegate(Portal portal, bool shouldActivate);

        public static event PortalTriggeredDelegate OnPortalTriggered;
        public static Portal ActivePortal { get; private set; }

        [SerializeField]
        private GameObject[] _activatedContent;

        [SerializeField]
        private GameObject[] _deactivatedContent;

        [SerializeField]
        private UnityEvent OnActivated;

        [SerializeField]
        private UnityEvent OnDeactivated;

        [SerializeField]
        [ReadOnly]
        private bool _isActivated = false;


        private TriggerState _triggerState;

        private void Awake()
        {
            ApplyContentVisibility();
        }

        private void OnEnable()
        {
            OnPortalTriggered += OnPortalTriggeredHandler;
        }

        private void OnDisable()
        {
            if (_isActivated)
            {
                OnPortalTriggered?.Invoke(this, false);
            }

            OnPortalTriggered -= OnPortalTriggeredHandler;
        }

        public void HandleTrigger(PortalTrigger trigger, Collider otherCollider, bool hasEntered)
        {
            var oldState = _triggerState;
            if (trigger.TriggerType == TriggerType.Front)
            {
                _triggerState.IsInFrontTrigger = hasEntered;
            }
            else if (trigger.TriggerType == TriggerType.Back)
            {
                _triggerState.IsInBackTrigger = hasEntered;
            }
            else if (trigger.TriggerType == TriggerType.Center)
            {
                _triggerState.IsInCenterTrigger = hasEntered;
            }


            // move into portal 
            if (oldState.IsOutsideTriggers && _triggerState.IsInBackTrigger)
            {
                Trigger();
            }

            // leave portal to inside
            if (_triggerState.IsOutsideTriggers && oldState.IsInFrontTrigger)
            {
                if (!_isActivated)
                {
                    Trigger();
                }
            }

            // move out of portal 
            if (_triggerState.IsOutsideTriggers && oldState.IsInBackTrigger)
            {
                if (_isActivated)
                {
                    Trigger();
                }
            }
        }

        private void OnPortalTriggeredHandler(Portal portal, bool shouldActivate)
        {
            if (shouldActivate)
            {
                if (portal == this && !_isActivated)
                {
                    ActivateContent();
                }
                else if (portal != this && _isActivated)
                {
                    DeactivateContent();
                }
            }
            else if (portal == this)
            {
                DeactivateContent();
            }
        }

        private void ActivateContent()
        {
            if (_isActivated)
            {
                return;
            }

            ActivePortal = this;
            _isActivated = true;
            ApplyContentVisibility();
            OnActivated?.Invoke();
        }

        private void DeactivateContent()
        {
            if (!_isActivated)
            {
                return;
            }

            if (ActivePortal == this)
            {
                ActivePortal = null;
            }

            _isActivated = false;
            ApplyContentVisibility();
            OnDeactivated?.Invoke();
        }

        private void ApplyContentVisibility()
        {
            foreach (var go in _activatedContent)
            {
                go.SetActive(_isActivated);
            }

            foreach (var go in _deactivatedContent)
            {
                go.SetActive(!_isActivated);
            }
        }

        [ContextMenu("Trigger")]
        public void Trigger()
        {
            OnPortalTriggered?.Invoke(this, !_isActivated);
        }

        private struct TriggerState
        {
            public bool IsInFrontTrigger;
            public bool IsInCenterTrigger;
            public bool IsInBackTrigger;

            public bool IsOutsideTriggers => (!IsInFrontTrigger && !IsInBackTrigger && !IsInCenterTrigger);
        }
    }
}

using System;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations;
using ff.common.entity;
using ff.utils;
using UnityEngine;

namespace ff.ar_rh_spurlab.GrayScaler
{
    public class Portal : MonoBehaviour
    {
        public delegate void PortalTriggeredDelegate(Portal portal, bool shouldActivate);

        public static event PortalTriggeredDelegate OnPortalTriggered;
        public static Portal ActivePortal { get; private set; }

        public event Action OnEnter;

        public event Action OnExit;

        [SerializeField]
        private LocalizedString _portalEntryLabelOverride;

        [SerializeField]
        private SetLocalizedText _portalEntryText;

        [SerializeField]
        [ReadOnly]
        private bool _isActivated = false;

        private TriggerState _triggerState;

        private void OnEnable()
        {
            OnPortalTriggered += OnPortalTriggeredHandler;


            if (_portalEntryLabelOverride.HasDefaultValue())
            {
                _portalEntryText.SetLocalizedString(_portalEntryLabelOverride);
            }
            else
            {
                var owningLocation = GetComponentInParent<AugmentedLocation>();
                if (owningLocation && owningLocation.LocationData)
                {
                    _portalEntryText.SetLocalizedString(owningLocation.LocationData.Title);
                }
            }
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
            OnEnter?.Invoke();
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
            OnExit?.Invoke();
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

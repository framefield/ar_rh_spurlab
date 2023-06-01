using System.Collections.Generic;
using ff.common.statemachine;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationTracking : MonoBehaviour, IActiveInStateContent, ITriggerSource
    {
        [SerializeField]
        private Trigger _trackedTrigger;

        [SerializeField]
        private Trigger _untrackedTrigger;


        private LocationController _locationController;
        private AugmentedLocation _currentLocation;
        private StateMachine _stateMachine;
        private ITriggerSource _triggerSourceImplementation;

        public void Initialize()
        {
            _locationController = FindFirstObjectByType<LocationController>();
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _stateMachine = stateMachine;
            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler();
            }
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            if (_locationController)
            {
                _locationController.LocationChanged -= OnLocationChangedHandler;
            }
        }

        private void OnLocationChangedHandler()
        {
            if (_currentLocation)
            {
                _currentLocation.OnTrackingChanged -= TrackingChangedHandler;
            }

            _currentLocation = _locationController.CurrentLocation;
            if (_currentLocation)
            {
                _currentLocation.OnTrackingChanged += TrackingChangedHandler;
                TrackingChangedHandler(_currentLocation.IsTracking);
            }
            else
            {
                TrackingChangedHandler(false);
            }
        }

        private void TrackingChangedHandler(bool isTracking)
        {
            _stateMachine.ProcessTrigger(this, isTracking ? _trackedTrigger : _untrackedTrigger);
        }


        IEnumerable<Trigger> ITriggerSource.Triggers
        {
            get => new[] { _trackedTrigger, _untrackedTrigger };
        }

        void ITriggerSource.ProcessTrigger(Trigger trigger)
        {
            _stateMachine.ProcessTrigger(this, trigger);
        }

        StateMachine ITriggerSource.StateMachine => _stateMachine;
    }
}
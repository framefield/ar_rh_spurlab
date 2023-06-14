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
            _locationController = LocationController.FindFirst();
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _stateMachine = stateMachine;
            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler(ChangeSource.Unknown);
            }
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            if (_locationController)
            {
                _locationController.LocationChanged -= OnLocationChangedHandler;
            }
        }

        private void OnLocationChangedHandler(ChangeSource source)
        {
            if (_currentLocation)
            {
                _currentLocation.OnTrackingStateChanged -= TrackingStateChangedHandler;
            }

            _currentLocation = _locationController.CurrentLocation;
            if (_currentLocation)
            {
                _currentLocation.OnTrackingStateChanged += TrackingStateChangedHandler;
                TrackingStateChangedHandler(_currentLocation.TrackingData.State);
            }
            else
            {
                TrackingStateChangedHandler(LocationTrackingState.None);
            }
        }

        private void TrackingStateChangedHandler(LocationTrackingState state)
        {
            _stateMachine.ProcessTrigger(this,
                state == LocationTrackingState.TrackingCalibration ? _trackedTrigger : _untrackedTrigger);
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
using ff.common.statemachine;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationSelection : MonoBehaviour, IActiveInStateContent
    {
        [SerializeField]
        private LocationSelectionUi _locationSelectionUiPrefab;

        private LocationController _locationController;

        private LocationSelectionUi _locationSelectionUi;
        private StateMachine _stateMachine;

        public void Initialize()
        {
            _locationSelectionUi = Instantiate(_locationSelectionUiPrefab, transform);
            _locationSelectionUi.SetVisibility(false);
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _locationController = stateMachine.GetComponent<LocationController>();
            _stateMachine = stateMachine;
            _locationSelectionUi.SetVisibility(true);
            _locationSelectionUi.SetOptions(_locationController.AvailableLocations);
            _locationSelectionUi.OnLocationSelected += OnLocationSelected;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _locationSelectionUi.SetVisibility(false);
            _locationSelectionUi.OnLocationSelected -= OnLocationSelected;
        }

        private void OnLocationSelected(LocationData locationData)
        {
            _locationController.SetLocation(locationData);
            _stateMachine.Continue();
        }
    }
}
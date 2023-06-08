using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.statemachine;
using UnityEngine;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationLocationSelection : MonoBehaviour, IActiveInStateContent
    {
        [SerializeField]
        private LocationSelectionUi _locationSelectionUiPrefab;

        private CalibrationController _calibrationController;

        private LocationSelectionUi _locationSelectionUi;

        private StateMachine _stateMachine;

        public void Initialize()
        {
            _locationSelectionUi = Instantiate(_locationSelectionUiPrefab, transform);
            _locationSelectionUi.SetVisibility(false);
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _stateMachine = stateMachine;
            
            if (!_calibrationController)
            {
                _calibrationController = stateMachine.GetComponent<CalibrationController>();
            }
            
            _calibrationController.SetLocation(SharedCalibrationContext.ActiveLocation);
            
            _locationSelectionUi.SetVisibility(true);
            _locationSelectionUi.SetOptions(_calibrationController.AvailableLocations);
            _locationSelectionUi.OnLocationSelected += OnLocationSelected;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _locationSelectionUi.SetVisibility(false);
            _locationSelectionUi.OnLocationSelected -= OnLocationSelected;
        }

        private void OnLocationSelected(LocationData locationData)
        {
            _calibrationController.SetLocation(SharedCalibrationContext.ActiveLocation);
            _stateMachine.Continue();
        }
    }
}
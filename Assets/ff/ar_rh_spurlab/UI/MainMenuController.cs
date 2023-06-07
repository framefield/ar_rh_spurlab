using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.statemachine;
using ff.common.ui;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private string _calibrationSceneName = "Calibration";

        [SerializeField]
        private Button _calibrateButton;

        [SerializeField]
        private Button _backButton;

        [SerializeField]
        private Button _appMenuButton;


        [SerializeField]
        private AppMenuController _appMenuController;

        
        public void Initialize(LocationController locationController, StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _locationController = locationController;
            _appMenuController.Initialize(locationController);
            locationController.LocationChanged += LocationChangedHandler;

            _calibrateButton.onClick.AddListener(CalibrateButtonClickedHandler);
            _backButton.onClick.AddListener(BackButtonClickedHandler);
            _appMenuButton.onClick.AddListener(AppMenuButtonClickedHandler);
            _appMenuController.OnClose += () => ToggleAppMenuOpen(false);
            
            // Open by default to enforce location selection.
            // This should probably be a setting in the long term 
            ToggleAppMenuOpen(true);
        }

        private void LocationChangedHandler()
        {
            var isCalibrationPossible = _locationController.CurrentLocation.LocationData != null;
            _calibrateButton.GetComponent<Hidable>().IsVisible = isCalibrationPossible;     // TODO: save component?
        }

        private void CalibrateButtonClickedHandler()
        {
            SharedCalibrationContext.ActiveLocation = _locationController.CurrentLocation.LocationData;
            SceneManager.LoadScene(_calibrationSceneName);
        }
        
        private void AppMenuButtonClickedHandler()
        {
            ToggleAppMenuOpen(true);
        }

        private void BackButtonClickedHandler()
        {
            _stateMachine.Reset();
        }

        private void ToggleAppMenuOpen(bool shouldBeOpen)
        {
            _appMenuController.IsVisible = shouldBeOpen;
            _appMenuButton.gameObject.SetActive(!shouldBeOpen);
        }
        
        private StateMachine _stateMachine;
        private LocationController _locationController;
    }
}
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.statemachine;
using ff.common.ui;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI
{
    public class UiController : MonoBehaviour
    {
        [SerializeField]
        private Hidable _canvasHidable;

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

            _appMenuButton.onClick.AddListener(AppMenuButtonClickedHandler);
            _appMenuController.OnClose += () => ToggleAppMenuOpen(false);

            OnIsPlayingChangedHandler(LocationTimelineManager.IsPlaying);
            LocationTimelineManager.IsPlayingChanged += OnIsPlayingChangedHandler;
        }

        private void OnIsPlayingChangedHandler(bool isPlaying)
        {
            _canvasHidable.IsVisible = !isPlaying;
            ToggleAppMenuOpen(false);
        }

        private void LocationChangedHandler()
        {
            var isCalibrationPossible = _locationController.CurrentLocation.LocationData != null;
            _appMenuController.SetIsCalibrationPossible(isCalibrationPossible);
        }

        private void AppMenuButtonClickedHandler()
        {
            ToggleAppMenuOpen(true);
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
using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class TrackingQualityController : MonoBehaviour
    {
        private LocationController _locationController;
        private AugmentedLocation _currentLocation;

        [SerializeField]
        private TMP_Text _calibrationMessageText;

        [SerializeField]
        private TMP_Text _percentageText;

        [SerializeField]
        private Image _progressIndicator;

        public void Awake()
        {
            _locationController = FindFirstObjectByType<LocationController>();
        }

        public void OnEnable()
        {
            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler();
            }
        }

        public void OnDisable()
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
                _currentLocation.OnTrackingDataChanged -= TrackingDataChangedHandler;
            }

            _currentLocation = _locationController.CurrentLocation;
            if (_currentLocation)
            {
                _currentLocation.OnTrackingDataChanged += TrackingDataChangedHandler;
                TrackingDataChangedHandler(_currentLocation.TrackingData);
            }
            else
            {
                TrackingDataChangedHandler(new LocationTrackingData());
            }
        }

        private void TrackingDataChangedHandler(LocationTrackingData state)
        {
            _progressIndicator.fillAmount = state.Quality;
            _percentageText.text = state.Quality.ToString("P0");
            _calibrationMessageText.text = state.CalibrationMessage;
        }
    }
}

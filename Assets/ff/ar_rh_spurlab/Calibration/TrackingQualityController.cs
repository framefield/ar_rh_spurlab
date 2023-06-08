using System;
using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if UNITY_IOS
using UnityEngine.XR.ARKit;

#endif

namespace ff.ar_rh_spurlab.Calibration
{
    public class TrackingQualityController : MonoBehaviour
    {
        private LocationController _locationController;
        private AugmentedLocation _currentLocation;

        [SerializeField]
        private ARSession _arSession;

        [SerializeField]
        private TMP_Text _calibrationMessageText;

        [FormerlySerializedAs("_worldTrackingMessageText")]
        [SerializeField]
        private TMP_Text _trackingMessageText;

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

        private void Update()
        {
            var worldMappingState = "simulator";
            var notTrackingReason = ARSession.notTrackingReason;
#if UNITY_IOS && !UNITY_EDITOR
            if (_arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                worldMappingState = sessionSubsystem.worldMappingStatus.ToString();
            }
#endif
            _trackingMessageText.text = $"World Mapping: {worldMappingState}"
                                        + (notTrackingReason != NotTrackingReason.None
                                            ? $" - Tracking:  {notTrackingReason.ToString()}"
                                            : " - Tracking: Okay");
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
            _calibrationMessageText.text = state.CalibrationMessage ?? "Calibrated";
        }
    }
}
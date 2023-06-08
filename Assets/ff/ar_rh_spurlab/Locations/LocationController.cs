using System;
using System.IO;
using ff.ar_rh_spurlab.AR;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.UI;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationController : MonoBehaviour
    {
        [SerializeField]
        private LocationData[] _availableLocations;

        [Header("Scene References")]
        [SerializeField]
        private StateMachine _stateMachine;

        [SerializeField]
        private ARSession _arSession;

        [SerializeField]
        private ARAnchorManager _arAnchorManager;

        [SerializeField]
        private Transform _xrOrigin;

        [FormerlySerializedAs("_mainMenuController")]
        [SerializeField]
        private UiController _uiController;
        
        private CalibrationARAnchorManager _calibrationARAnchorManager;


        private AugmentedLocation _augmentedLocation;
        public LocationData[] AvailableLocations => _availableLocations;

        public event Action LocationChanged;
        public AugmentedLocation CurrentLocation => _augmentedLocation;

        
        
        private void Start()
        {
            if (!_stateMachine)
            {
                Debug.LogError("LocationController: StateMachine is not set!", this);
                return;
            }

            if (!_arSession)
            {
                Debug.LogError("LocationController: ARSession is not set!", this);
                return;
            }

            if (!_arAnchorManager)
            {
                Debug.LogError("LocationController: ARAnchorManager is not set!", this);
                return;
            }

            if (!_xrOrigin)
            {
                Debug.LogError("LocationController: XrOrigin is not set!", this);
                return;
            }

            _calibrationARAnchorManager =
                new CalibrationARAnchorManager(_arAnchorManager, CalibrationARAnchorManager.Mode.Tracking);
            _stateMachine.Initialize();
            _uiController.Initialize(this, _stateMachine);
        }

        public void ResetLocation()
        {
            if (_augmentedLocation)
            {
                Destroy(_augmentedLocation.gameObject);
                _augmentedLocation = null;
                _calibrationARAnchorManager.Reset();
            }
        }

        public bool SetLocation(LocationData locationData)
        {
            var calibrationData = CalibrationData.TryLoad(locationData.Title);

            if (calibrationData == null)
            {
                Debug.LogError($"Location is not calibrated: {locationData.Title}", this);
                return false;
            }

            if (_augmentedLocation)
            {
                Destroy(_augmentedLocation.gameObject);
                _augmentedLocation = null;
            }

            _augmentedLocation = Instantiate(locationData.ContentPrefab, _xrOrigin);
            _augmentedLocation.Initialize(calibrationData, locationData);
            _calibrationARAnchorManager.SetCalibrationData(calibrationData);
#if UNITY_IOS
            var directoryPath = Path.Combine(Application.persistentDataPath, calibrationData.Name);
            var filePath = Path.Combine(directoryPath, "my_session.worldmap");

            StartCoroutine(ARWorldMapController.Load(_arSession, filePath));
#endif
            LocationChanged?.Invoke();
            return true;
        }
    }
}

using System;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.UI;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationController : MonoBehaviour
    {
        [SerializeField]
        private string _calibrationSceneName = "Calibration";

        [SerializeField]
        private LocationData _defaultLocationData;

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

            if (SharedCalibrationContext.ActiveLocation == null)
            {
                SharedCalibrationContext.ActiveLocation = _defaultLocationData;
            }

            _uiController.Initialize(this, _stateMachine);
            if (SharedCalibrationContext.ActiveLocation)
            {
                SetLocation(SharedCalibrationContext.ActiveLocation);
            }

            _stateMachine.Initialize();
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
            SharedCalibrationContext.ActiveLocation = locationData;

            var isCalibrated = CalibrationData.CalibrationDataExists(locationData.Id);
            if (!isCalibrated)
            {
                CalibrateActiveLocation();
                return false;
            }

            var calibrationData = CalibrationData.TryLoad(locationData.Id);

            if (calibrationData == null)
            {
                Debug.LogError($"Location is not calibrated: {locationData.Id}", this);
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
            var directoryPath = System.IO.Path.Combine(Application.persistentDataPath, calibrationData.Id);
            var filePath = System.IO.Path.Combine(directoryPath, "my_session.worldmap");

            StartCoroutine(ff.ar_rh_spurlab.AR.ARWorldMapController.Load(_arSession, filePath));
#endif
            LocationChanged?.Invoke();
            return true;
        }

        public void CalibrateActiveLocation()
        {
            if (SharedCalibrationContext.ActiveLocation == null)
            {
                Debug.LogError("No active location set!", this);
                return;
            }

            SceneManager.LoadScene(_calibrationSceneName);
        }

#if UNITY_EDITOR
        [ContextMenu("Toggle Missing Point From Calibration")]
        public void ToggleMissingPointFromCalibration()
        {
            _calibrationARAnchorManager.ToggleMissingPointFromCalibration();
        }

        [ContextMenu("Toggle Jitter Calibration")]
        public void ToggleJitterCalibration()
        {
            _calibrationARAnchorManager.ToggleJitterCalibration();
        }
#endif
    }
}
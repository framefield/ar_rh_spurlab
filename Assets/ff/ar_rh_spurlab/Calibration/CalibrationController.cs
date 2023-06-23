using System;
using System.IO;
using System.Threading.Tasks;
using ff.ar_rh_spurlab.AR;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationController : MonoBehaviour
    {
        [SerializeField]
        private CalibrationUi _calibrationUiPrefab;

        [Header("Scene References")]
        [SerializeField]
        private StateMachine _stateMachine;

        [SerializeField]
        private ARSession _arSession;

        [SerializeField]
        private ARRaycastManager _arRaycastManager;

        [SerializeField]
        private ARAnchorManager _arAnchorManager;

        [SerializeField]
        private Transform _xrOrigin;

        private CalibrationARAnchorManager _calibrationARAnchorManager;

        private CalibrationUi _calibrationUi;

        private AugmentedLocation _augmentedLocation;

        public CalibrationData CalibrationData { get; private set; }
        public LocationData LocationData => _augmentedLocation ? _augmentedLocation.LocationData : null;

        public ARSession Session => _arSession;
        public ARRaycastManager RaycastManager => _arRaycastManager;
        public ARAnchorManager AnchorManager => _arAnchorManager;
        public Transform XrOrigin => _xrOrigin;

        private void Start()
        {
            if (!SharedLocationContext.ActiveLocation)
            {
                Debug.LogError("CalibrationController: ActiveLocation is not set!", this);
                return;
            }

            Debug.Log($"CalibrationController: will load location {SharedLocationContext.ActiveLocation.Id}");

            if (!_stateMachine)
            {
                Debug.LogError("CalibrationController: StateMachine is not set!", this);
                return;
            }

            if (!_arSession)
            {
                Debug.LogError("CalibrationController: ARSession is not set!", this);
                return;
            }

            if (!_arRaycastManager)
            {
                Debug.LogError("CalibrationController: ARRaycastManager is not set!", this);
                return;
            }

            if (!_arAnchorManager)
            {
                Debug.LogError("CalibrationController: ARAnchorManager is not set!", this);
                return;
            }

            if (!_xrOrigin)
            {
                Debug.LogError("CalibrationController: XrOrigin is not set!", this);
                return;
            }

            if (!_calibrationUiPrefab)
            {
                Debug.LogError("CalibrationController: CalibrationUiPrefab is not set!", this);
                return;
            }

            _calibrationUi = Instantiate(_calibrationUiPrefab, transform);
            _calibrationUi.SetSession(_arSession);

            _calibrationARAnchorManager =
                new CalibrationARAnchorManager(_arAnchorManager, CalibrationARAnchorManager.Mode.Calibrating);
            SetLocation(SharedLocationContext.ActiveLocation);
            _stateMachine.Initialize();
        }

        private void SetLocation(LocationData locationData)
        {
            Debug.Log($"CalibrationController: setting locationId '{locationData.Id}'");

            CalibrationData = new CalibrationData(locationData.Id);

            if (_augmentedLocation)
            {
                Destroy(_augmentedLocation.gameObject);
            }

            if (!_xrOrigin)
            {
                Debug.LogError("XR origin not set!");
                return;
            }

            if (!locationData.CalibrationPrefab)
            {
                Debug.LogError($"Location {locationData.Id} is has no calibrationPrefab!");
                return;
            }

            _augmentedLocation = Instantiate(locationData.CalibrationPrefab, _xrOrigin);
            _augmentedLocation.Initialize(CalibrationData, locationData);
            _calibrationUi.SetCalibrationData(CalibrationData);
            _calibrationARAnchorManager.SetCalibrationData(CalibrationData);
        }

        public async Task SaveCalibrationData()
        {
            var directoryPath = Path.Combine(Application.persistentDataPath, CalibrationData.Id);
            var filePath = Path.Combine(directoryPath, "my_session.worldmap");

            try
            {
                _calibrationUi.SetIsSaving(true);
#if UNITY_IOS
                await ARWorldMapController.Save(_arSession, filePath);
#endif
                await CalibrationData.Store(directoryPath);
            }
            finally
            {
                _calibrationUi.SetIsSaving(false);
            }
        }

        public void ResetCalibration()
        {
            if (Session.didStart)
            {
                Session.Reset();
            }

            _calibrationARAnchorManager.Reset();
        }
    }
}

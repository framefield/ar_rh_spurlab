using System.IO;
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

        [SerializeField]
        private LocationData[] _availableLocations;

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
        public LocationData LocationData => _augmentedLocation.LocationData;

        public ARSession Session => _arSession;
        public ARRaycastManager RaycastManager => _arRaycastManager;
        public ARAnchorManager AnchorManager => _arAnchorManager;
        public LocationData[] AvailableLocations => _availableLocations;

        public Transform XrOrigin => _xrOrigin;

        private void Start()
        {
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
            _calibrationUi.OnRestartButtonClicked += () => _stateMachine.Reset();
            _calibrationUi.SetSession(_arSession);

            _calibrationARAnchorManager =
                new CalibrationARAnchorManager(_arAnchorManager, CalibrationARAnchorManager.Mode.Calibrating);
            _stateMachine.Initialize();
            SetLocation(SharedCalibrationContext.ActiveLocation);
        }

        public void SetLocation(LocationData locationData)
        {
            CalibrationData = new CalibrationData(locationData.Title);

            if (_augmentedLocation)
            {
                Destroy(_augmentedLocation.gameObject);
            }

            _augmentedLocation = Instantiate(locationData.CalibrationPrefab, _xrOrigin);
            _augmentedLocation.Initialize(CalibrationData, locationData);
            _calibrationUi.SetCalibrationData(CalibrationData);
            _calibrationARAnchorManager.SetCalibrationData(CalibrationData);
        }

        public void SaveCalibrationData()
        {
            var directoryPath = Path.Combine(Application.persistentDataPath, CalibrationData.Name);
            var filePath = Path.Combine(directoryPath, "my_session.worldmap");

#if UNITY_IOS
            StartCoroutine(ARWorldMapController.Save(_arSession, filePath));
#endif

            CalibrationData.Store(directoryPath);
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
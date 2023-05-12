using ff.ar_rh_spurlab.Locations;
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
        private Location _locationPrefab;

        [Header("Scene References")]
        [SerializeField]
        private StateMachine _stateMachine;

        [SerializeField]
        private ARRaycastManager _arRaycastManager;

        [SerializeField]
        private ARAnchorManager _arAnchorManager;

        [SerializeField]
        private Transform _xrOrigin;

        private CalibrationUi _calibrationUi;

        private Location _location;

        public CalibrationData CalibrationData { get; private set; }

        public ARRaycastManager RaycastManager => _arRaycastManager;
        public ARAnchorManager AnchorManager => _arAnchorManager;
        public Transform XrOrigin => _xrOrigin;

        private void Start()
        {
            if (!_arRaycastManager)
            {
                Debug.LogError("CalibrationController: ARRaycastManager is not set!");
                return;
            }

            if (!_arAnchorManager)
            {
                Debug.LogError("CalibrationController: ARAnchorManager is not set!");
                return;
            }

            if (!_xrOrigin)
            {
                Debug.LogError("CalibrationController: XrOrigin is not set!");
                return;
            }

            if (!_calibrationUiPrefab)
            {
                Debug.LogError("CalibrationController: CalibrationUiPrefab is not set!");
                return;
            }

            _calibrationUi = Instantiate(_calibrationUiPrefab, transform);
            _calibrationUi.OnRestartButtonClicked += () => _stateMachine.Reset();

            _stateMachine.Initialize();
        }

        public void SetLocation(string locationName)
        {
            CalibrationData = new CalibrationData(locationName,
                new Vector3[]
                    { new(10.615f, 6.802f, 2.355f), new(8.448f, 6.809f, 4.812f), new(8.734f, 9.936f, 2.708f) });

            if (!_location)
            {
                _location = Instantiate(_locationPrefab, _xrOrigin);
            }

            _location.SetCalibrationData(CalibrationData);
        }
    }
}
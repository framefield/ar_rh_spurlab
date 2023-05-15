using System.IO;
using ff.ar_rh_spurlab.AR;
using ff.ar_rh_spurlab.Calibration;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationController : MonoBehaviour
    {
        [SerializeField]
        private LocationData _locationData;

        [Header("Scene References")]
        [SerializeField]
        private StateMachine _stateMachine;

        [SerializeField]
        private ARSession _arSession;

        [SerializeField]
        private ARAnchorManager _arAnchorManager;

        [SerializeField]
        private Transform _xrOrigin;

        private CalibrationARAnchorManager _calibrationARAnchorManager;


        private Location _location;

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
        }

        public void SetLocation(string locationName)
        {
            var calibrationData = CalibrationData.TryLoad(locationName);

            if (calibrationData == null)
            {
                Debug.LogError($"Location is not calibrated: {locationName}", this);
                return;
            }

            if (!_location)
            {
                _location = Instantiate(_locationData._prefab, _xrOrigin);
            }

            _location.Initialize(calibrationData, _locationData);
            _calibrationARAnchorManager.SetCalibrationData(calibrationData);

#if UNITY_IOS
            var directoryPath = Path.Combine(Application.persistentDataPath, calibrationData.Name);
            var filePath = Path.Combine(directoryPath, "my_session.worldmap");

            StartCoroutine(ARWorldMapController.Load(_arSession, filePath));
#endif
        }
    }
}
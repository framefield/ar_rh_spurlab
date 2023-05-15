#if UNITY_IOS && !UNITY_EDITOR
using System.IO;
using ff.ar_rh_spurlab.AR;
#endif

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
        private Transform _xrOrigin;


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

            if (!_xrOrigin)
            {
                Debug.LogError("LocationController: XrOrigin is not set!", this);
                return;
            }

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

#if UNITY_IOS && !UNITY_EDITOR
            var directoryPath = Path.Combine(Application.persistentDataPath, calibrationData.Name);
            var filePath = Path.Combine(directoryPath, "my_session.worldmap");

            StartCoroutine(ARWorldMapController.Load(_arSession, filePath, OnArWorldMapLoadedHandler));
#else
            _location.CalibrationData.SearchForMarkers();
#endif
        }

        private void OnArWorldMapLoadedHandler()
        {
            _location.CalibrationData.SearchForMarkers();
        }
    }
}
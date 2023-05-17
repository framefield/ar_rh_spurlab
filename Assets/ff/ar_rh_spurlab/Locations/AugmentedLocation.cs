using ff.ar_rh_spurlab.Calibration;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class AugmentedLocation : MonoBehaviour
    {
        public LocationData LocationData { get; private set; }
        public CalibrationData CalibrationData { get; private set; }

        private void Update()
        {
            if (CalibrationData?.AreAnchorsReady != true)
            {
                foreach (var trackedLocationContent in _trackedLocationContents)
                {
                    trackedLocationContent.SetIsTracked(false);
                }

                return;
            }

            var (xrOriginTLocationOrigin, isValid) =
                CalibrationCalculator.GetXrOriginTLocationOrigin(CalibrationData, LocationData);

            transform.position = xrOriginTLocationOrigin.GetPosition();
            transform.rotation = xrOriginTLocationOrigin.rotation;

            foreach (var trackedLocationContent in _trackedLocationContents)
            {
                trackedLocationContent.SetIsTracked(isValid);
            }
        }

        public void Initialize(CalibrationData calibrationData, LocationData locationData)
        {
            CalibrationData = calibrationData;
            LocationData = locationData;

            _trackedLocationContents = GetComponentsInChildren<ITrackedLocationContent>();
            Debug.Log($"Location.Initialize: {name} has {_trackedLocationContents.Length} tracked location contents");

            foreach (var trackedLocationContent in _trackedLocationContents)
            {
                trackedLocationContent.Initialize();
            }
        }

        private ITrackedLocationContent[] _trackedLocationContents;
    }
}
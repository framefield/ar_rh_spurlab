using ff.ar_rh_spurlab.Calibration;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class Location : MonoBehaviour
    {
        [SerializeField]
        private GameObject _content;

        public LocationData LocationData { get; private set; }
        public CalibrationData CalibrationData { get; private set; }

        private void Update()
        {
            if (CalibrationData?.AreAnchorsReady != true)
            {
                _content.gameObject.SetActive(false);
                return;
            }

            var (xrOriginTLocationOrigin, isValid) =
                CalibrationCalculator.GetXrOriginTLocationOrigin(CalibrationData, LocationData);

            transform.position = xrOriginTLocationOrigin.GetPosition();
            transform.rotation = xrOriginTLocationOrigin.rotation;
            _content.gameObject.SetActive(true);
        }

        public void Initialize(CalibrationData calibrationData, LocationData locationData)
        {
            CalibrationData = calibrationData;
            LocationData = locationData;
        }
    }
}
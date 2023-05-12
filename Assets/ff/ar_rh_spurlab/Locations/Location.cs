using ff.ar_rh_spurlab.Calibration;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class Location : MonoBehaviour
    {
        [SerializeField]
        private GameObject _content;

        private CalibrationData _calibrationData;

        private void Update()
        {
            if (_calibrationData?.IsValid != true)
            {
                _content.gameObject.SetActive(false);
                return;
            }

            var (xrOriginTLocationOrigin, isValid) = CalibrationCalculator.GetXrOriginTLocationOrigin(_calibrationData);

            transform.position = xrOriginTLocationOrigin.GetPosition();
            transform.rotation = xrOriginTLocationOrigin.rotation;
            _content.gameObject.SetActive(true);
        }

        public void SetCalibrationData(CalibrationData calibrationData)
        {
            _calibrationData = calibrationData;
        }
    }
}
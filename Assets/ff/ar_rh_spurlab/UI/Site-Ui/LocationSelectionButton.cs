using System;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class LocationSelectionButton : MonoBehaviour
    {
        public event Action<LocationData> OnLocationButtonClicked;

        [Header("Prefab References")]
        [SerializeField]
        private Button _button;

        [SerializeField]
        private GameObject _active;

        [SerializeField]
        private GameObject _inactive;

        [SerializeField]
        private TMP_Text[] _titleTexts;

        [SerializeField]
        private TMP_Text[] _labelTexts;

        [Header("Asset Reference")]
        [SerializeField]
        private LocationData _locationData;

        public void Initialize(LocationData locationData, LocationController locationController, char labelChar)
        {
            _locationController = locationController;
            _locationData = locationData;
            _label = labelChar.ToString();
            locationController.LocationChanged += LocationChangedHandler;
            UpdateVisuals();
            
            _button.onClick.AddListener(OnButtonClicked);
        }

        // TODO: Unclear, if this is required.
// #if UNITY_EDITOR
//         private void OnValidate()
//         {
//             UpdateVisuals();
//         }
// #endif

        private void OnButtonClicked()
        {
            OnLocationButtonClicked?.Invoke(_locationData);
        }



        private void LocationChangedHandler()
        {
            UpdateVisuals();
        }


        private void UpdateVisuals()
        {
            _active.SetActive(IsLocationActive);
            _inactive.SetActive(!IsLocationActive);

            if (_locationData == null)
                return;

            foreach (var labelText in _labelTexts)
            {
                labelText.text = _label;
            }

            foreach (var titleText in _titleTexts)
            {
                var calibrationMissingSuffix = CalibrationData.CalibrationDataExists(_locationData.Title) 
                    ? string.Empty 
                    : "\n(not calibrated)";            
                titleText.text = _locationData.Title  + calibrationMissingSuffix;
            }
        }

        private bool IsLocationActive => _locationController.CurrentLocation && _locationController.CurrentLocation.LocationData == _locationData;
        private string _label = "A";
        private LocationController _locationController;
    }
}
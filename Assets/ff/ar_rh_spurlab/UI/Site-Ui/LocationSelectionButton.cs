using System;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations;
using ff.common.entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class LocationSelectionButton : AbstractLocalizable
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

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (!_locationData)
                return;

            if (!_locationData.Title.TryGetValue(locale, out var title))
                return;

            foreach (var titleText in _titleTexts)
            {
                var calibrationMissingSuffix = CalibrationData.CalibrationDataExists(_locationData.Id)
                    ? string.Empty
                    : "\n(not calibrated)";

                titleText.text = title + calibrationMissingSuffix;
            }
        }

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

            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
        }

        private bool IsLocationActive => _locationController.CurrentLocation &&
                                         _locationController.CurrentLocation.LocationData == _locationData;

        private string _label = "A";
        private LocationController _locationController;
    }
}
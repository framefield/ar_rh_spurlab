using System;
using System.Collections.Generic;
using ff.ar_rh_spurlab.Calibration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationOptionData : TMP_Dropdown.OptionData
    {
        public LocationOptionData(LocationData location)
        {
            LocationData = location;
            var isCalibrated = CalibrationData.CalibrationDataExists(location._id);
            text = $"{location._id}{(isCalibrated ? "" : " (not calibrated)")}";
        }

        public LocationData LocationData { get; set; }
    }

    public class LocationSelectionUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private TMP_Dropdown _dropdown;

        [SerializeField]
        private Button _selectButton;

        private void Start()
        {
            _selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        public event Action<LocationData> OnLocationSelected;

        private void OnSelectButtonClicked()
        {
            var selectedOptionIndex = _dropdown.value;
            var selectedOption = _dropdown.options[selectedOptionIndex];

            if (selectedOption is LocationOptionData locationOptionData)
            {
                OnLocationSelected?.Invoke(locationOptionData.LocationData);
            }
        }

        public void SetOptions(IEnumerable<LocationData> locationData)
        {
            _dropdown.options.Clear();

            foreach (var location in locationData)
            {
                _dropdown.options.Add(new LocationOptionData(location));
            }
        }

        public void SetVisibility(bool visible)
        {
            _canvas.enabled = visible;
        }
    }
}
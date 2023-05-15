using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationSelectionUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private TMP_Dropdown _dropdown;

        [SerializeField]
        private Button _selectButton;


        private readonly Dictionary<string, LocationData> _locationDataByName = new();

        private void Start()
        {
            _selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        public event Action<LocationData> OnLocationSelected;

        private void OnSelectButtonClicked()
        {
            var selectedOptionIndex = _dropdown.value;
            var selectedOption = _dropdown.options[selectedOptionIndex];
            var locationName = selectedOption.text;
            var locationData = _locationDataByName[locationName];
            OnLocationSelected?.Invoke(locationData);
        }

        public void SetOptions(IEnumerable<LocationData> locationData)
        {
            _dropdown.options.Clear();
            _locationDataByName.Clear();

            foreach (var location in locationData)
            {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(location._name));
                _locationDataByName.Add(location._name, location);
            }
        }

        public void SetVisibility(bool visible)
        {
            _canvas.enabled = visible;
        }
    }
}
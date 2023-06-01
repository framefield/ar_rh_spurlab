using System;
using TMPro;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class SetTextFromActiveLocation : MonoBehaviour
    {
        private LocationController _locationController;
        private TMP_Text _text;

        private void Awake()
        {
            _locationController = FindFirstObjectByType<LocationController>();
            _text = GetComponent<TMP_Text>();

            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler();
            }
        }

        private void OnLocationChangedHandler()
        {
            if (_locationController && _text)
            {
                _text.text = _locationController.CurrentLocation != null
                    ? _locationController.CurrentLocation.LocationData.Title
                    : "No Location";
            }
        }
    }
}
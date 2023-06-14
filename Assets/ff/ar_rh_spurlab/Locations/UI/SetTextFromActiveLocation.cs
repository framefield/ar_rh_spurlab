using ff.ar_rh_spurlab.Localization;
using ff.common.entity;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations.UI
{
    public class SetTextFromActiveLocation : SetLocalizedText
    {
        [SerializeField]
        private LocalizedString _noLocationString = new LocalizedString("No Location Selected");

        private LocationController _locationController;

        private void Awake()
        {
            _locationController = LocationController.FindFirst();
            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler(ChangeSource.Unknown);
            }
        }

        private void OnLocationChangedHandler(ChangeSource source)
        {
            if (_locationController)
            {
                _localizedString = _locationController.CurrentLocation != null
                    ? _locationController.CurrentLocation.LocationData.Title
                    : _noLocationString;
                OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
            }
        }
    }
}
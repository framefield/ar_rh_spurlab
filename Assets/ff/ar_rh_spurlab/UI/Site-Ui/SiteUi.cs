using System;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations;
using ff.common.entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class SiteUi : AbstractLocalizable
    {
        public event Action<SiteData> OnMapButtonClicked;
        public event Action<SiteData, LocationData> OnLocationButtonClicked;

        [Header("Prefab References")]
        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private Button _mapButton;

        [SerializeField]
        private Transform _locationsContainer;

        [Header("Asset Reference")]
        [SerializeField]
        private LocationSelectionButton _locationButtonPrefab;


        public void Initialize(SiteData siteData, LocationController locationController)
        {
            _mapButton.onClick.AddListener(OnMapButtonClickedHandler);

            _siteData = siteData;

            if (_siteData == null)
            {
                Debug.LogWarning("can't initialize SiteUi without valid data", this);
                return;
            }

            _mapButton.interactable = _siteData.MapContentPrefab;

            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);

            var label = 'A';
            foreach (var locationData in _siteData.Locations)
            {
                var newLocationButton = Instantiate(_locationButtonPrefab, _locationsContainer);
                newLocationButton.Initialize(locationData, locationController, label++);
                newLocationButton.OnLocationButtonClicked +=
                    data => OnLocationButtonClicked?.Invoke(_siteData, data);
            }
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (!_siteData)
                return;

            if (_siteData.Title.TryGetValue(locale, out var title))
            {
                _titleText.text = title;
            }
        }

        private void OnMapButtonClickedHandler()
        {
            OnMapButtonClicked?.Invoke(_siteData);
        }

        private SiteData _siteData;
    }
}
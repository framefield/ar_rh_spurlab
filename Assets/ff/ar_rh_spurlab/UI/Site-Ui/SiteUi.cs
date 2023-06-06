using System;
using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class SiteUi : MonoBehaviour
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

        private void OnEnable()
        {
            _mapButton.onClick.AddListener(OnMapButtonClickedHandler);
        }

        private void OnDisable()
        {
            _mapButton.onClick.RemoveListener(OnMapButtonClickedHandler);
        }

        private void OnMapButtonClickedHandler()
        {
            OnMapButtonClicked?.Invoke(_siteData);
        }

        public void SetSiteData(SiteData siteData)
        {
            _siteData = siteData;

            foreach (Transform child in _locationsContainer)
            {
                Destroy(child.gameObject);
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_siteData == null)
                return;

            _titleText.text = _siteData.Name.ToUpper();

            var label = 'A';
            foreach (var locationData in _siteData.Locations)
            {
                var locationButton = Instantiate(_locationButtonPrefab, _locationsContainer);
                locationButton.SetLabel(label++.ToString());
                locationButton.SetLocationData(locationData);
                locationButton.OnLocationButtonClicked +=
                    data => OnLocationButtonClicked?.Invoke(_siteData, data);
            }
        }

        private SiteData _siteData;
    }
}
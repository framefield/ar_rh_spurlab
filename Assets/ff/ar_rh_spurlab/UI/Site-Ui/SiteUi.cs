using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class SiteUi : MonoBehaviour
    {
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


        public void SetSiteData(Locations.SiteData siteData)
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
            }
        }

        private Locations.SiteData _siteData;
    }
}
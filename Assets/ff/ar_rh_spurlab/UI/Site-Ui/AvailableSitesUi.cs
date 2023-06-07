using System;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class AvailableSitesUi : MonoBehaviour
    {
        public event Action<SiteData> OnMapButtonClicked;
        public event Action<SiteData, LocationData> OnLocationButtonClicked;

        [Header("Asset References")]
        [SerializeField]
        private AvailableSites _availableSites;

        [SerializeField]
        private SiteUi _siteUiPrefab;

        public void Initialize(LocationController locationController)
        {
            foreach (var site in _availableSites.Sites)
            {
                var newSiteUi = Instantiate(_siteUiPrefab, transform);
                newSiteUi.Initialize(site, locationController);
                newSiteUi.OnMapButtonClicked += (data) => OnMapButtonClicked?.Invoke(data);
                newSiteUi.OnLocationButtonClicked += (siteData, locationData) =>
                    OnLocationButtonClicked?.Invoke(siteData, locationData);
            }
        }
    }
}
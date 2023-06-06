using ff.ar_rh_spurlab.Locations;
using UnityEngine;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class AvailableSitesUi : MonoBehaviour
    {
        [Header("Asset References")]
        [SerializeField]
        private AvailableSites _availableSites;

        [SerializeField]
        private SiteUi _siteUiPrefab;

        private void Start()
        {
            foreach (var site in _availableSites.Sites)
            {
                var siteUi = Instantiate(_siteUiPrefab, transform);
                siteUi.SetSiteData(site);
            }
        }
    }
}
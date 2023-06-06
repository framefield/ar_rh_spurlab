using System;
using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class AvailableSitesUi : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement _layoutElement;

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
                _siteUis.Add(siteUi);
            }
        }

        private void Update()
        {
            var height = _siteUis.Sum(siteUi => siteUi.GetComponent<RectTransform>().rect.height);
            _layoutElement.minHeight = height;
        }

        private readonly List<SiteUi> _siteUis = new();
    }
}
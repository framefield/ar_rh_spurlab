using System;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI
{
    public class AppMenuController : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Button _contactButton;

        [SerializeField]
        private Button _termsButton;

        [SerializeField]
        private Button _resetButton;

        [SerializeField]
        private AvailableSitesUi _availableSitesUi;

        private void OnEnable()
        {
            _availableSitesUi.OnMapButtonClicked += OnMapButtonClickedHandler;
            _availableSitesUi.OnLocationButtonClicked += OnLocationButtonClickedHandler;
            _closeButton.onClick.AddListener(OnCloseButtonClickedHandler);
            _contactButton.onClick.AddListener(OnContactButtonClickedHandler);
            _termsButton.onClick.AddListener(OnTermsButtonClickedHandler);
            _resetButton.onClick.AddListener(OnResetButtonClickedHandler);
        }

        private void OnDisable()
        {
            _availableSitesUi.OnMapButtonClicked -= OnMapButtonClickedHandler;
            _availableSitesUi.OnLocationButtonClicked -= OnLocationButtonClickedHandler;
            _closeButton.onClick.RemoveListener(OnCloseButtonClickedHandler);
            _contactButton.onClick.RemoveListener(OnContactButtonClickedHandler);
            _termsButton.onClick.RemoveListener(OnTermsButtonClickedHandler);
            _resetButton.onClick.RemoveListener(OnResetButtonClickedHandler);
        }

        private void OnMapButtonClickedHandler(SiteData data)
        {
            Debug.Log($"Map button clicked for site {data.Name}");
        }

        private void OnLocationButtonClickedHandler(SiteData siteData, LocationData locationData)
        {
            Debug.Log($"Location button clicked for site {siteData.Name} and location {locationData.Title}");
        }

        private void OnCloseButtonClickedHandler()
        {
            Debug.Log("Close button clicked");
        }

        private void OnContactButtonClickedHandler()
        {
            Debug.Log("Contact button clicked");
        }

        private void OnTermsButtonClickedHandler()
        {
            Debug.Log("Terms button clicked");
        }

        private void OnResetButtonClickedHandler()
        {
            Debug.Log("Reset button clicked");
        }
    }
}
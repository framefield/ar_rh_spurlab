using System;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI
{
    public class AppMenuController : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Button _contactButton;

        [SerializeField]
        private Button _termsButton;

        [SerializeField]
        private Button _resetButton;

        [SerializeField]
        private Button _calibrateButton;

        [SerializeField]
        private AvailableSitesUi _availableSitesUi;

        public event Action OnClose;

        public bool IsVisible
        {
            get => _hidable.IsVisible;
            set => _hidable.IsVisible = value;
        }

        public void Initialize(LocationController locationController)
        {
            _locationController = locationController;
            _availableSitesUi.Initialize(locationController);

            _availableSitesUi.OnMapButtonClicked += OnMapButtonClickedHandler;
            _availableSitesUi.OnLocationButtonClicked += OnLocationButtonClickedHandler;
            _closeButton.onClick.AddListener(OnCloseButtonClickedHandler);
            _contactButton.onClick.AddListener(OnContactButtonClickedHandler);
            _termsButton.onClick.AddListener(OnTermsButtonClickedHandler);
            _resetButton.onClick.AddListener(OnResetButtonClickedHandler);
            _calibrateButton.onClick.AddListener(OnCalibrateButtonClickedHandler);
        }


        public void SetIsCalibrationPossible(bool isCalibrationPossible)
        {
            _calibrateButton.GetComponent<Hidable>().IsVisible = isCalibrationPossible; // TODO: save component?
        }

        private void OnMapButtonClickedHandler(SiteData data)
        {
            Debug.Log($"Map button clicked for site {data.Id}");
        }

        private void OnLocationButtonClickedHandler(SiteData siteData, LocationData locationData)
        {
            Debug.Log($"Location button clicked for site {siteData.Id} and location {locationData.Title}");

            _locationController.SetLocation(locationData);
        }

        private void OnCloseButtonClickedHandler()
        {
            OnClose?.Invoke();
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

        private void OnCalibrateButtonClickedHandler()
        {
            _locationController.CalibrateActiveLocation();
        }

        private LocationController _locationController;
    }
}
using System;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        }        


        private void OnMapButtonClickedHandler(SiteData data)
        {
            Debug.Log($"Map button clicked for site {data.Name}");
        }

        private void OnLocationButtonClickedHandler(SiteData siteData, LocationData locationData)
        {
            Debug.Log($"Location button clicked for site {siteData.Name} and location {locationData.Title}");
            
            var isCalibrated = CalibrationData.CalibrationDataExists(locationData.Title);
            if (isCalibrated)
            {
                _locationController.SetLocation(locationData);
            }
            else
            {
                SharedCalibrationContext.ActiveLocation = locationData;
                SceneManager.LoadScene("Calibration");
            }
            
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

        private LocationController _locationController;

    }
}
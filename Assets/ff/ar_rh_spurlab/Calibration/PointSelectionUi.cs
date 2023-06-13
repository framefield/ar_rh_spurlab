using System;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class PointSelectionUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _continueButton;

        [SerializeField]
        private RawImage _referenceImage;

        
        public event Action OnContinueButtonClicked;

        
        public void Initialize(PointSelectionController pointSelectionController)
        {
            _pointSelectionController = pointSelectionController;
        }
        
        
        public void UpdateUi(LocationData locationData, int activePointIndex)
        {
            if (activePointIndex < locationData.PointsInformation.Length)
            {
                _referenceImage.gameObject.SetActive(true);
                _referenceImage.texture = locationData.PointsInformation[activePointIndex].ScreenImage;
            }
            else
            {
                _referenceImage.gameObject.SetActive(false);
                //_referenceImage.material.mainTexture = null;
            }
        }

        private void Start()
        {
            _continueButton.onClick.AddListener(ContinueButtonClickedHandler);
        }

        private void Update()
        {
            if (!_pointSelectionController)
            {
                return;
            }

            _continueButton.interactable = _pointSelectionController.IsReady;
        }


        private void ContinueButtonClickedHandler()
        {
            OnContinueButtonClicked?.Invoke();
        }

        private PointSelectionController _pointSelectionController;


    }
}
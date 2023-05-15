using System;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class PointSelectionUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _continueButton;

        private PointSelectionController _pointSelectionController;

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

        public event Action OnContinueButtonClicked;

        private void ContinueButtonClickedHandler()
        {
            OnContinueButtonClicked?.Invoke();
        }

        public void SetPointSelectionController(PointSelectionController pointSelectionController)
        {
            _pointSelectionController = pointSelectionController;
        }
    }
}
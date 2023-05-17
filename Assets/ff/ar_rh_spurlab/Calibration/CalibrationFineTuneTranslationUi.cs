using System;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationFineTuneTranslationUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _continueButton;

        [SerializeField]
        private Button _backButton;

        private void Start()
        {
            _continueButton.onClick.AddListener(ContinueButtonClickedHandler);
            _backButton.onClick.AddListener(BackButtonClickedHandler);
        }

        public event Action OnContinueButtonClicked;
        public event Action OnBackButtonClicked;

        private void ContinueButtonClickedHandler()
        {
            OnContinueButtonClicked?.Invoke();
        }

        private void BackButtonClickedHandler()
        {
            OnBackButtonClicked?.Invoke();
        }
    }
}
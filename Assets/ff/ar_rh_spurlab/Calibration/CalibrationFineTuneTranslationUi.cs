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

        private void Start()
        {
            _continueButton.onClick.AddListener(ContinueButtonClickedHandler);
        }

        public event Action OnContinueButtonClicked;

        private void ContinueButtonClickedHandler()
        {
            OnContinueButtonClicked?.Invoke();
        }
    }
}
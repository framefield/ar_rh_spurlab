using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _restartButton;

        [Header("Calibration Data Texts")]
        [SerializeField]
        private TMP_Text _calibrationNameText;

        [SerializeField]
        private TMP_Text _calibrationValidText;

        private CalibrationData _calibrationData;

        private void Start()
        {
            _restartButton.onClick.AddListener(RestartButtonClickedHandler);
        }

        private void Update()
        {
            _calibrationNameText.text = $"Name: '{_calibrationData?.Name}'";
            _calibrationValidText.text = $"Valid: {_calibrationData?.IsValid}";
        }

        public event Action OnRestartButtonClicked;

        private void RestartButtonClickedHandler()
        {
            OnRestartButtonClicked?.Invoke();
        }

        public void SetCalibrationData(CalibrationData calibrationData)
        {
            _calibrationData = calibrationData;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _restartButton;

        private void Start()
        {
            _restartButton.onClick.AddListener(RestartButtonClickedHandler);
        }

        public event Action OnRestartButtonClicked;

        private void RestartButtonClickedHandler()
        {
            OnRestartButtonClicked?.Invoke();
        }
    }
}
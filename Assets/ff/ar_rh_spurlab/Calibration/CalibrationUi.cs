using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationUi : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField]
        private Button _restartButton;

        [SerializeField]
        private string _mainSceneName = "Main";

        [SerializeField]
        private Button _goToMainSceneButton;

        [Header("Calibration Data Texts")]
        [SerializeField]
        private TMP_Text _calibrationNameText;

        [SerializeField]
        private TMP_Text _calibrationValidText;


        [Header("World Map Texts")]
        [SerializeField]
        private TMP_Text _worldMapStatusText;

        private ARSession _arSession;
        private CalibrationData _calibrationData;

        private void Start()
        {
            _restartButton.onClick.AddListener(RestartButtonClickedHandler);
            _goToMainSceneButton.onClick.AddListener(GoToMainSceneButtonClickedHandler);
        }

        private void Update()
        {
            UpdateCalibrationDataStatusUi();
            UpdateMappingStatusUi();
        }

        private void GoToMainSceneButtonClickedHandler()
        {
            SceneManager.LoadScene(_mainSceneName);
        }

        private void UpdateCalibrationDataStatusUi()
        {
            _calibrationNameText.text = $"Name: '{_calibrationData?.Name}'";
            _calibrationValidText.text = $"Valid: {_calibrationData?.AreAnchorsReady}";
        }

        private void UpdateMappingStatusUi()
        {
            if (!_arSession)
            {
                return;
            }

#if UNITY_IOS
            if (_arSession.subsystem is not ARKitSessionSubsystem sessionSubsystem)
            {
                return;
            }

            _worldMapStatusText.text = $"Mapping Status: {sessionSubsystem.worldMappingStatus}";
#endif
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

        public void SetSession(ARSession arSession)
        {
            _arSession = arSession;
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationUi : MonoBehaviour
    {
        [Header("Prefab references")]
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
            _calibrationNameText.text = $"Id: '{_calibrationData?.Id}'";
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

using ff.common.statemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private string _calibrationSceneName = "Calibration";

        [SerializeField]
        private Button _calibrateButton;

        [SerializeField]
        private Button _backButton;

        [SerializeField]
        private StateMachine _stateMachine;


        private void Start()
        {
            _calibrateButton.onClick.AddListener(CalibrateButtonClickedHandler);
            _backButton.onClick.AddListener(BackButtonClickedHandler);
        }

        private void CalibrateButtonClickedHandler()
        {
            SceneManager.LoadScene(_calibrationSceneName);
        }

        private void BackButtonClickedHandler()
        {
            _stateMachine.Reset();
        }
    }
}
using ff.ar_rh_spurlab.Locations;
using ff.common.statemachine;
using ff.common.ui;
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
        private AppMenuController _appMenuController;

        
        public void Initialize(LocationController locationController, StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _appMenuController.Initialize(locationController);

            _calibrateButton.onClick.AddListener(CalibrateButtonClickedHandler);
            _backButton.onClick.AddListener(BackButtonClickedHandler);
            _calibrateButton.GetComponent<Hidable>().IsVisible = true;
        }

        private void CalibrateButtonClickedHandler()
        {
            SceneManager.LoadScene(_calibrationSceneName);
        }

        private void BackButtonClickedHandler()
        {
            _stateMachine.Reset();
        }
        
        private StateMachine _stateMachine;
    }
}
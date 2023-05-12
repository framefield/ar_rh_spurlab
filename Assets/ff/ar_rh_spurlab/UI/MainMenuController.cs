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

        private void Start()
        {
            _calibrateButton.onClick.AddListener(CalibrateButtonClickedHandler);
        }

        private void CalibrateButtonClickedHandler()
        {
            SceneManager.LoadScene(_calibrationSceneName);
        }
    }
}
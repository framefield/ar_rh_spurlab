using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public class ARAnchorInfo : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        private ARAnchor _arAnchor;

        private void Update()
        {
            if (!_arAnchor)
            {
                _arAnchor = GetComponent<ARAnchor>();
            }

            UpdateUi();
        }

        private void UpdateUi()
        {
            if (!_arAnchor)
            {
                _text.text = "ARAnchor: null";
                return;
            }

            _text.text = $"ARAnchor: {transform.position} \n state:{_arAnchor.trackingState}";
        }
    }
}
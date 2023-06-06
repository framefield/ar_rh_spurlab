using TMPro;
using UnityEngine;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    public class LocationSelectionButton : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private TMP_Text[] _titleTexts;

        [SerializeField]
        private TMP_Text[] _labelTexts;

        [SerializeField]
        private GameObject _active;

        [SerializeField]
        private GameObject _inactive;

        [Header("Asset Reference")]
        [SerializeField]
        private Locations.LocationData _locationData;


#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateVisuals();
        }
#endif

        public void SetLocationData(Locations.LocationData locationData)
        {
            _locationData = locationData;
            UpdateVisuals();
        }

        public void SetLabel(string label)
        {
            _label = label;
            UpdateVisuals();
        }

        public void SetIsActive(bool isActive)
        {
            _isLocationActive = isActive;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            _active.SetActive(_isLocationActive);
            _inactive.SetActive(!_isLocationActive);

            if (_locationData == null)
                return;

            foreach (var labelText in _labelTexts)
            {
                labelText.text = _label;
            }

            foreach (var titleText in _titleTexts)
            {
                titleText.text = _locationData.Title;
            }
        }

        private bool _isLocationActive;
        private string _label = "A";
    }
}
using System;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class MapLocationUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _labelText;

        [SerializeField]
        private Hidable _visitedHidable;

        [SerializeField]
        private TMP_FontAsset _activeFontAsset;

        [SerializeField]
        private TMP_FontAsset _inactiveFontAsset;

        public void Initialize(LocationData locationData, char label, PlaceableUIContainer placeableUiContainer,
            Vector3 worldPosition)
        {
            _locationData = locationData;
            gameObject.name = $"{locationData.Id} - MapLocationUi";
            _labelText.text = label.ToString();

            _worldPosition = worldPosition;
            _placeableUiContainer = placeableUiContainer;
            _rectTransform = GetComponent<RectTransform>();
            UpdateStates();
        }

        private void OnEnable()
        {
            UpdateStates();
        }

        private void UpdateStates()
        {
            if (!_locationData)
            {
                return;
            }

            var isVisited = SharedLocationContext.VisitedLocationIds.Contains(_locationData.Id);
            var isActiveLocation = SharedLocationContext.ActiveLocation.Id == _locationData.Id;

            _visitedHidable.IsVisible = isVisited;
            _labelText.font = isActiveLocation ? _activeFontAsset : _inactiveFontAsset;
        }

        private void Update()
        {
            if (!_rectTransform || _placeableUiContainer == null)
                return;

            var screenPosition = _placeableUiContainer.WorldToUiPosition(_worldPosition);
            _rectTransform.anchoredPosition = screenPosition;
        }

        private Vector3 _worldPosition;
        private PlaceableUIContainer _placeableUiContainer;
        private RectTransform _rectTransform;
        private LocationData _locationData;
    }
}
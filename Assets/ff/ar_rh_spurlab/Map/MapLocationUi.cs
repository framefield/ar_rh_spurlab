using System;
using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;

namespace ff.ar_rh_spurlab.Map
{
    public class MapLocationUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _labelText;


        public void Initialize(LocationData locationData, char label, PlaceableUIContainer placeableUiContainer,
            Vector3 worldPosition)
        {
            gameObject.name = $"{locationData.Id} - MapLocationUi";
            _labelText.text = label.ToString();

            _worldPosition = worldPosition;
            _placeableUiContainer = placeableUiContainer;
            _rectTransform = GetComponent<RectTransform>();
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
    }
}
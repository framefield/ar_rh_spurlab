using System;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.entity;
using ff.common.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class MapLocationUi : AbstractLocalizable
    {
        [Header("Prefab References")]
        [SerializeField]
        private TMP_Text _labelText;

        [SerializeField]
        private Hidable _visitedHidable;

        [SerializeField]
        private Button _toggleDetailsButton;

        [SerializeField]
        private Hidable _detailsHidable;

        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private RectTransform _tiltedTransform;

        [Header("Asset References")]
        [SerializeField]
        private TMP_FontAsset _activeFontAsset;

        [SerializeField]
        private TMP_FontAsset _inactiveFontAsset;

        public void Initialize(LocationData locationData, char label, PlaceableUIContainer placeableUiContainer,
            Vector3 worldPosition, float tiltAngle)
        {
            _locationData = locationData;
            gameObject.name = $"{locationData.Id} - MapLocationUi";
            _labelText.text = label.ToString();

            _worldPosition = worldPosition;
            _tiltedTransform.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
            _placeableUiContainer = placeableUiContainer;
            _rectTransform = GetComponent<RectTransform>();

            _toggleDetailsButton.onClick.AddListener(OnToggleDetailsButtonClickedHandler);
            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
            UpdateStates();
        }

        private void OnToggleDetailsButtonClickedHandler()
        {
            _detailsHidable.IsVisible = !_detailsHidable.IsVisible;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateStates();
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (!_locationData)
            {
                return;
            }

            if (_locationData.Title.TryGetValue(locale, out var title))
            {
                _titleText.text = title;
            }
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
using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations;
using ff.common.entity;
using ff.common.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class MapUi : AbstractLocalizable
    {
        [Header("Prefab References")]
        [SerializeField]
        private TMP_Text _siteTitleText;

        [SerializeField]
        private RawImage _backgroundImage;

        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private RectTransform _locationsContainer;

        [SerializeField]
        private MapLocationUi _mapLocationUiPrefab;

        [SerializeField]
        private MapCameraInteraction _mapCameraInteraction;

        [SerializeField]
        private MapPositionIndicator _mapPositionIndicator;

        [Header("Asset References")]
        [SerializeField]
        private AvailableSites _availableSites;

        private void Start()
        {
            _closeButton.onClick.AddListener(CloseOnClickHandler);

            if (MapUiController.MapUiInstance != null)
                Debug.LogError(
                    "MapUiController: Multiple instances of MapUi detected! Ignoring all but first.",
                    gameObject);
            else
                MapUiController.MapUiInstance = this;
        }

        private void CloseOnClickHandler()
        {
            if (_activeSiteData)
            {
                var mapContent = _mapContentBySiteId[_activeSiteData.Id];
                mapContent.SetVisibility(false);
            }

            _hidable.IsVisible = false;
        }

        public void ShowLocationMap(string locationId)
        {
            var siteData = _availableSites.Sites.First(s => s.Locations.Any(l => l.Id == locationId));
            ShowSiteMap(siteData);
        }

        public void ShowSiteMap(string siteId)
        {
            var siteData = _availableSites.Sites.First(s => s.Id == siteId);
            ShowSiteMap(siteData);
        }

        public void ShowSiteMap(SiteData siteData)
        {
            if (siteData == null)
            {
                Debug.LogError($"MapUi: Can't show map for null site data!", this);
                return;
            }

            _hidable.IsVisible = true;

            if (_activeSiteData != null && _activeSiteData.Id == siteData.Id)
            {
                var activeMapContent = _mapContentBySiteId[_activeSiteData.Id];
                activeMapContent.SetVisibility(true);
                return;
            }

            if (_activeSiteData != null)
            {
                var previousMapContent = _mapContentBySiteId[_activeSiteData.Id];
                previousMapContent.SetVisibility(false);
            }

            if (!_mapContentBySiteId.ContainsKey(siteData.Id))
            {
                var newMapContent = Instantiate(siteData.MapContentPrefab);
                _mapContentBySiteId.Add(siteData.Id, newMapContent);
            }

            _activeSiteData = siteData;
            var mapContent = _mapContentBySiteId[siteData.Id];
            mapContent.SetVisibility(true);
            _backgroundImage.texture = mapContent.RenderTexture;
            _mapCameraInteraction.SetMapCamera(mapContent.MapCamera);
            _mapPositionIndicator.SetMapContent(mapContent);
            UpdateText();
            ReplaceLocationUis(siteData, mapContent);
        }

        private void UpdateText()
        {
            if (_activeSiteData == null)
            {
                Debug.LogWarning("can't update text without valid data", this);
                return;
            }

            _activeSiteData.Title.TryGetValue(ApplicationLocale.Instance.CurrentLocale, out var title);
            _siteTitleText.text = title;
        }

        private void ReplaceLocationUis(SiteData siteData, MapContent mapContent)
        {
            // todo pooling
            foreach (var mapLocationUi in _mapLocationUis)
            {
                Destroy(mapLocationUi.gameObject);
            }

            _mapLocationUis.Clear();

            var placeableUiContainer = new PlaceableUIContainer(_locationsContainer, mapContent.MapCamera.Camera);
            var label = 'A';
            foreach (var locationData in siteData.Locations)
            {
                var mapLocationUi = Instantiate(_mapLocationUiPrefab, _locationsContainer);
                var worldPosition = mapContent.GeoPositionToWorldPosition(locationData.GeoPosition);
                mapLocationUi.Initialize(locationData,
                    label++,
                    placeableUiContainer,
                    worldPosition,
                    mapContent.MapCamera.TiltAngle);
                _mapLocationUis.Add(mapLocationUi);
            }
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            UpdateText();
        }

        private SiteData _activeSiteData;
        private readonly Dictionary<string, MapContent> _mapContentBySiteId = new();
        private readonly List<MapLocationUi> _mapLocationUis = new();
    }
}
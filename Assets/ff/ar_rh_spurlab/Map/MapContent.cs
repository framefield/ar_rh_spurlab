using ff.ar_rh_spurlab.Locations;
using ff.ar_rh_spurlab.Positioning;
using UnityEngine;

namespace ff.ar_rh_spurlab.Map
{
    public class MapContent : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private MapCamera _mapCamera;

        [SerializeField]
        private GeoReference _geoReference;

        public RenderTexture RenderTexture => GetRenderTexture();

        public MapCamera MapCamera => _mapCamera;

        private RenderTexture GetRenderTexture()
        {
            if (!_renderTexture)
            {
                _renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                _mapCamera.Camera.targetTexture = _renderTexture;
            }

            return _renderTexture;
        }

        public void SetVisibility(bool isVisible)
        {
            _mapCamera.SetVisibility(isVisible);

            if (!_trackedLocationContent)
            {
                // use simple tracked location content for hiding renderers and colliders, instead of reimplementing the same logic.
                _trackedLocationContent = gameObject.AddComponent<SimpleTrackedLocationContent>();
                _trackedLocationContent.Initialize();
            }

            _trackedLocationContent.SetIsTracked(isVisible);
        }

        public Vector3 GeoPositionToWorldPosition(GeoPosition geoPosition)
        {
            return _geoReference.TransformToWorldPosition(geoPosition);
        }

        private RenderTexture _renderTexture;
        private SimpleTrackedLocationContent _trackedLocationContent;
    }
}
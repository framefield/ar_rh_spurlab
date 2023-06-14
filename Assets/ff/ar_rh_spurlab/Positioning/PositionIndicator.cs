using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    [RequireComponent(typeof(GeoPositioned))]
    public class PositionIndicator : MonoBehaviour
    {
        private GeoPositioned _geoPositioned;
        private PositioningService _service;

        private void OnEnable()
        {
            _service = PositioningService.FindFirst();
            if (_service)
            {
                _geoPositioned = GetComponent<GeoPositioned>();
                _service.OnPositionChanged += OnPositionChanged;
            }
            else
            {
                Debug.LogError("No PositioningService found in scene");
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (_service)
            {
                _service.OnPositionChanged -= OnPositionChanged;
            }
        }

        private void OnPositionChanged(PositioningInfo info)
        {
            _geoPositioned.GeoPosition = info.GeoPosition;
            _geoPositioned.Heading = (float)info.Heading;
        }
    }
}

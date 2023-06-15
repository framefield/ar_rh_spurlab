using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class PlaceableUIContainer
    {
        public PlaceableUIContainer(Transform transform, Camera cam)
        {
            var canvasScaler = transform.GetComponentInParent<CanvasScaler>();

            if (canvasScaler)
            {
                _canvasScaleFactor = canvasScaler.transform.localScale.x;
            }

            _cam = cam;
        }

        public Vector2 WorldToUiPosition(Vector3 worldReference, bool pixelAlign = true)
        {
            Vector2 canvasPosition = _cam.WorldToScreenPoint(worldReference) / _canvasScaleFactor;
            if (pixelAlign)
            {
                canvasPosition.x = Mathf.Round(canvasPosition.x * _canvasScaleFactor) / _canvasScaleFactor;
                canvasPosition.y = Mathf.Round(canvasPosition.y * _canvasScaleFactor) / _canvasScaleFactor;
            }

            return canvasPosition;
        }

        private readonly float _canvasScaleFactor;
        private readonly Camera _cam;
    }
}
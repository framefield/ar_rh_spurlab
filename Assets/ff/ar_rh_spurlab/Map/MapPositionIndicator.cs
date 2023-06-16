using System;
using ff.ar_rh_spurlab.Positioning;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class MapPositionIndicator : MonoBehaviour
    {
        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private RectTransform _contentContainer;

        [SerializeField]
        private RectTransform _accuracyContainer;

        [Header("Transition")]
        [SerializeField]
        private float _radiusTransitionSpeed = 5f;

        [SerializeField]
        private float _headingTransitionSpeed = 5f;

        [Header("Debug")]
        [SerializeField]
        private Vector3 _debugWorldPosition = Vector3.zero;

        [SerializeField]
        private float _debugAccuracy = 20f;

        [SerializeField]
        private float _debugHeading = 0f;

        [SerializeField]
        private RectTransform _debugImage;

        public void SetMapContent(MapContent mapContent)
        {
            _mapContent = mapContent;
            _placeableUiContainer = new PlaceableUIContainer(_contentContainer, _mapContent.MapCamera.Camera);
        }

        private void OnEnable()
        {
            _hidable.IsVisible = false;

            _service = PositioningService.FindFirst();
            if (_service)
            {
                _service.OnPositionChanged += OnPositionChangedHandler;
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
                _service.OnPositionChanged -= OnPositionChangedHandler;
            }
        }

        private void OnPositionChangedHandler(PositioningInfo info)
        {
            if (!_mapContent)
            {
                return;
            }

            _hidable.IsVisible = true;

            _worldPosition = _mapContent.GeoPositionToWorldPosition(info.GeoPosition);
            _accuracyReferencePosition = _worldPosition + Vector3.right * info.HorizontalAccuracy;
            _headingReferencePosition =
                _worldPosition + Quaternion.AngleAxis((float)-info.Heading, Vector3.up) * Vector3.forward;
        }

        private void UpdateDebugPositioning()
        {
            _hidable.IsVisible = true;
            _worldPosition = _debugWorldPosition;

            _accuracyReferencePosition = _worldPosition + Vector3.right * _debugAccuracy;
            _headingReferencePosition =
                _worldPosition + Quaternion.AngleAxis(-_debugHeading, Vector3.up) * Vector3.forward * 10f;
        }

        private void Update()
        {
#if UNITY_EDITOR
            UpdateDebugPositioning();
#endif

            if (!_mapContent && !_hidable.IsVisible)
            {
                return;
            }

            var screenPosition = _placeableUiContainer.WorldToUiPosition(_worldPosition);
            _contentContainer.anchoredPosition = screenPosition;

            var accuracyReferenceScreenPosition = _placeableUiContainer.WorldToUiPosition(_accuracyReferencePosition);
            var accuracyRadius = Vector2.Distance(screenPosition, accuracyReferenceScreenPosition);

            _accuracyValueTransition.TargetValue = accuracyRadius;
            var radius = _accuracyValueTransition.Update(Time.deltaTime, _radiusTransitionSpeed, Mathf.Lerp);

            _accuracyContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, radius * 2f);
            _accuracyContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, radius * 2f);


            var headingReferenceScreenPosition = _placeableUiContainer.WorldToUiPosition(_headingReferencePosition);
            _debugImage.anchoredPosition = headingReferenceScreenPosition;
            var heading = headingReferenceScreenPosition - screenPosition;
            var headingAngle = Vector3.SignedAngle(Vector2.up, heading, Vector3.forward);
            _headingAngleTransition.TargetValue = -headingAngle;
            var angle = _headingAngleTransition.Update(Time.deltaTime, _headingTransitionSpeed, Mathf.LerpAngle);


            _contentContainer.localRotation =
                Quaternion.Euler(_mapContent.MapCamera.TiltAngle, 0, angle);
        }


        private readonly ValueTransition<float> _accuracyValueTransition = new(0f);
        private readonly ValueTransition<float> _headingAngleTransition = new(0f);

        private MapContent _mapContent;
        private PositioningService _service;
        private PlaceableUIContainer _placeableUiContainer;
        private Vector3 _worldPosition;
        private Vector3 _accuracyReferencePosition;
        private Vector3 _headingReferencePosition;
    }
}
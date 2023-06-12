using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    [ExecuteInEditMode]
    public class GeoPositioned : MonoBehaviour
    {
        [SerializeField]
        private GeoReference _geoReference;

        [SerializeField]
        private GeoPosition _geoPosition = new GeoPosition(52.54087049633316, 13.386393768213711);

        [SerializeField]
        private float _heading;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        public float Heading
        {
            get => _heading;
            set
            {
                _heading = value;
                UpdateHeading();
            }
        }

        public GeoPosition GeoPosition
        {
            get => _geoPosition;
            set
            {
                _geoPosition = value;
                UpdateFromReference();
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            UpdateFromReference();
            UpdateHeading();
#endif
            Transform localTransform = transform;
            if (Application.isPlaying)
            {
                localTransform.position = Vector3.Lerp(localTransform.position, _targetPosition, 0.2f);
                localTransform.rotation = Quaternion.Lerp(localTransform.rotation, _targetRotation, 0.2f);
            }
            else
            {
                localTransform.position = _targetPosition;
                localTransform.rotation = _targetRotation;
            }
        }

        private void UpdateFromReference()
        {
            if (_geoReference)
            {
                _targetPosition = _geoReference.TransformToWorldPosition(_geoPosition);
            }
        }

        private void UpdateHeading()
        {
            _targetRotation = Quaternion.Euler(0, _heading, 0);
        }
    }
}
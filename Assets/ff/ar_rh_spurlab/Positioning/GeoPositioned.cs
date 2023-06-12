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

#if UNITY_EDITOR
        private void Update()
        {
            UpdateFromReference();
            UpdateHeading();
        }
#endif

        private void UpdateFromReference()
        {
            if (_geoReference)
            {
                transform.position = _geoReference.TransformToWorldPosition(_geoPosition);
            }
        }

        private void UpdateHeading()
        {
            transform.rotation = Quaternion.Euler(0, -_heading, 0);
        }
    }
}

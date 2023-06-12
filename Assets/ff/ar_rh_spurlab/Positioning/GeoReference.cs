using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    public class GeoReference : MonoBehaviour
    {
        [SerializeField]
        private GeoPosition _geoPosition = new GeoPosition(52.54087049633316, 13.386393768213711);

        public Vector3 TransformToWorldPosition(GeoPosition geoPosition)
        {
            var enu = _geoPosition.CalculateEnu(geoPosition);
            return transform.localToWorldMatrix.MultiplyPoint(enu);
        }
    }
}

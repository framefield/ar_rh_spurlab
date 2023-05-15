using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "LocationData", order = 0)]
    public class LocationData : ScriptableObject
    {
        public static readonly int NumberOfReferencePoints = 3;

        public Location _prefab;
        public Vector3[] _pointsInLocationOrigin;

        private void OnValidate()
        {
            if (_pointsInLocationOrigin.Length == NumberOfReferencePoints)
            {
                return;
            }

            var validPoints = new Vector3[NumberOfReferencePoints];
            for (var i = 0; i < Mathf.Min(NumberOfReferencePoints, _pointsInLocationOrigin.Length); ++i)
                validPoints[i] = _pointsInLocationOrigin[i];
            _pointsInLocationOrigin = validPoints;
        }
    }
}
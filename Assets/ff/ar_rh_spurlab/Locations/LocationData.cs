using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "LocationData", order = 0)]
    public class LocationData : ScriptableObject
    {
        public static readonly int NumberOfReferencePoints = 3;

        public string Title = "Location Placeholder Name";
        public Location ContentPrefab;
        public Location CalibrationPrefab;
        public Vector3[] PointsInLocationOrigin;

        private void OnValidate()
        {
            if (PointsInLocationOrigin.Length == NumberOfReferencePoints)
            {
                return;
            }

            var validPoints = new Vector3[NumberOfReferencePoints];
            for (var i = 0; i < Mathf.Min(NumberOfReferencePoints, PointsInLocationOrigin.Length); ++i)
            {
                validPoints[i] = PointsInLocationOrigin[i];
            }

            PointsInLocationOrigin = validPoints;
        }
    }
}
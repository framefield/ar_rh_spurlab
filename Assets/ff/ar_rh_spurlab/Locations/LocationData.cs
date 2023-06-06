using ff.common.entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "LocationData", order = 0)]
    public class LocationData : ScriptableObject
    {
        public const int NumberOfReferencePoints = 3;

        public string Id => _id;
        public LocalizedString Title => _title;

        [FormerlySerializedAs("Title")]
        private LocalizedString _title = new("Location Name");

        [FormerlySerializedAs("Title")]
        private string _id = "location-placeholder-id";

        public AugmentedLocation ContentPrefab;
        public AugmentedLocation CalibrationPrefab;
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

using System;
using ff.common.entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.Locations
{
    [Serializable]
    public struct PointInformation
    {
        public string Id;
        public LocalizedString Title;
        public Texture2D ScreenImage;
    }

    [CreateAssetMenu(fileName = "LocationData", menuName = "LocationData", order = 0)]
    public class LocationData : ScriptableObject
    {
        public const int NumberOfReferencePoints = 3;

        public string Id => _id;
        public LocalizedString Title => _title;

        [FormerlySerializedAs("Title")]
        [SerializeField]
        private string _id = "location-placeholder-id";

        [SerializeField]
        private LocalizedString _title = new("Location Name");

        public AugmentedLocation ContentPrefab;
        public AugmentedLocation CalibrationPrefab;
        public Vector3[] PointsInLocationOrigin;
        public PointInformation[] PointsInformation;

        private void OnValidate()
        {
            if (PointsInLocationOrigin.Length == NumberOfReferencePoints &&
                PointsInformation.Length == NumberOfReferencePoints)
            {
                return;
            }

            var validPoints = new Vector3[NumberOfReferencePoints];
            var validInformation = new PointInformation[NumberOfReferencePoints];

            Array.Copy(PointsInformation, validInformation,
                Math.Min(NumberOfReferencePoints, PointsInformation.Length));
            Array.Copy(PointsInLocationOrigin, validPoints,
                Math.Min(NumberOfReferencePoints, PointsInLocationOrigin.Length));

            PointsInLocationOrigin = validPoints;
            PointsInformation = validInformation;
        }
    }
}

using ff.ar_rh_spurlab.Map;
using ff.common.entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "SiteData", menuName = "SiteData", order = 0)]
    public class SiteData : ScriptableObject
    {
        public string Id => _id;
        public LocalizedString Title => _title;
        public MapContent MapContentPrefab => _mapContentPrefab;

        public LocationData[] Locations => _locations;

        [FormerlySerializedAs("_name")]
        [SerializeField]
        private string _id;

        [SerializeField]
        private MapContent _mapContentPrefab;

        [SerializeField]
        private LocalizedString _title;

        [SerializeField]
        private LocationData[] _locations;
    }
}
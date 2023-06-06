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

        public LocationData[] Locations => _locations;

        [FormerlySerializedAs("_name")]
        [SerializeField]
        private string _id;


        [SerializeField]
        private LocalizedString _title;

        [SerializeField]
        private LocationData[] _locations;
    }
}

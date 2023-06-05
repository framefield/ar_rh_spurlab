using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "SiteData", menuName = "SiteData", order = 0)]
    public class SiteData : ScriptableObject
    {
        public string Name => _name;
        public LocationData[] Locations => _locations;

        [SerializeField]
        private string _name;

        [SerializeField]
        private LocationData[] _locations;
    }
}
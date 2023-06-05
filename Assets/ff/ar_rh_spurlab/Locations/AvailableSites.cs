using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "AvailableSites", menuName = "AvailableSites", order = 0)]
    public class AvailableSites : ScriptableObject
    {
        public SiteData[] Sites => _sites;

        [SerializeField]
        private SiteData[] _sites;
    }
}
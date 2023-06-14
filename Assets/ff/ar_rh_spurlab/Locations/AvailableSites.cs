using JetBrains.Annotations;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    [CreateAssetMenu(fileName = "AvailableSites", menuName = "AvailableSites", order = 0)]
    public class AvailableSites : ScriptableObject
    {
        public SiteData[] Sites => _sites;

        [SerializeField]
        private SiteData[] _sites;

        [CanBeNull]
        public static AvailableSites LoadFromResources()
        {
            var availableSites = Resources.Load<AvailableSites>("AvailableSites");
            if (!availableSites)
            {
                Debug.LogError("AvailableSites.LoadFromResources could not load sites");
            }

            return availableSites;
        }
    }
}

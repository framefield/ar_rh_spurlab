using ff.ar_rh_spurlab.Locations;

namespace ff.ar_rh_spurlab.Map
{
    public static class MapUiController
    {
        public static MapUi MapUiInstance;

        public static void ShowMap(SiteData siteData)
        {
            if (MapUiInstance)
            {
                MapUiInstance.ShowMap(siteData);
            }
        }
    }
}
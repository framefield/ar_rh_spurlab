using ff.ar_rh_spurlab.Locations;

namespace ff.ar_rh_spurlab.Map
{
    public static class MapUiController
    {
        public static MapUi MapUiInstance;

        public static void ShowSiteMap(SiteData siteData)
        {
            if (MapUiInstance)
            {
                MapUiInstance.ShowSiteMap(siteData);
            }
        }

        public static void ShowSiteMap(string siteId)
        {
            if (MapUiInstance)
            {
                MapUiInstance.ShowSiteMap(siteId);
            }
        }

        public static void ShowLocationMap(string locationId)
        {
            if (MapUiInstance)
            {
                MapUiInstance.ShowLocationMap(locationId);
            }
        }
    }
}
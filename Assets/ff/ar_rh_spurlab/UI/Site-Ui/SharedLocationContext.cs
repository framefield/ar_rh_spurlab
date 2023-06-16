using System.Collections.Generic;
using ff.ar_rh_spurlab.Locations;

namespace ff.ar_rh_spurlab.UI.Site_Ui
{
    /// <summary>
    /// A simple work around to share data between scenes 
    /// </summary>
    public static class SharedLocationContext
    {
        public static LocationData ActiveLocation;

        public static readonly HashSet<string> VisitedLocationIds = new();
    }
}
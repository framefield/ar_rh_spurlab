using UnityEngine;

namespace ff.ar_rh_spurlab.Calibration
{
    [PreferBinarySerialization]
    public class WorldMap : ScriptableObject
    {
        [HideInInspector]
        public byte[] Bytes;
    }
}

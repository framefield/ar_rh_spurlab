using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ff.ar_rh_spurlab.LineBuildup
{
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point 
    {
        public Vector3 Position;
        public float W;
        public Vector4 Rotation;
    }
    
    [Serializable]
    public class PointList: ScriptableObject
    {
        [SerializeField]
        public Point[] Points;
    }
}

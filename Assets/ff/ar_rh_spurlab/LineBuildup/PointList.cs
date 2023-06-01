using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PointPosition
    {
        public float X;
        public float Y;
        public float Z;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PointQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public PointPosition Position;
        public float W;
        public PointQuaternion Rotation;
    }

    [Serializable]
    public class PointList : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public Point[] Points;

        [SerializeField]
        public Bounds Bounds;

        public void GenerateBounds()
        {
            if (Points.Length == 0)
            {
                Bounds = new Bounds();
                return;
            }

            Bounds = new Bounds(new Vector3(-Points[0].Position.X, Points[0].Position.Y, Points[0].Position.Z),
                Vector3.zero);
            foreach (var point in Points)
            {
                if (float.IsNaN(point.W))
                {
                    continue;
                }

                Bounds.Encapsulate(new Vector3(-point.Position.X, point.Position.Y, point.Position.Z));
            }
        }

        public void Fill(ComputeBuffer pointsBuffer)
        {
            var data = new float[Points.Length * 8];
            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                data[i * 8 + 0] = -point.Position.X;
                data[i * 8 + 1] = point.Position.Y;
                data[i * 8 + 2] = point.Position.Z;
                data[i * 8 + 3] = point.W;
                data[i * 8 + 4] = point.Rotation.X;
                data[i * 8 + 5] = point.Rotation.Y;
                data[i * 8 + 6] = point.Rotation.Z;
                data[i * 8 + 7] = point.Rotation.W;
            }

            pointsBuffer.SetData(data);
        }
    }
}

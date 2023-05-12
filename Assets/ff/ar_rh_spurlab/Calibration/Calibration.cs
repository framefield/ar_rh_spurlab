using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public static class CalibrationCalculator
    {
        public static (Matrix4x4 xrOriginTLocationOrigin, bool isValid) GetXrOriginTLocationOrigin(
            CalibrationData calibrationData)
        {
            if (calibrationData?.IsValid != true)
            {
                return (Matrix4x4.identity, false);
            }

            Vector3[] pointsInXROrigin =
            {
                calibrationData.Marker1.Position,
                calibrationData.Marker2.Position,
                calibrationData.Marker3.Position
            };
            var (xrOriginTLocationOrigin, matchingDeviation) =
                CalculateTransformFromAToB(calibrationData.PointsInLocationOrigin, pointsInXROrigin);
            
            return (Matrix4x4.Translate(calibrationData.Offset) * xrOriginTLocationOrigin, true);
        }

        private static (Matrix4x4, float) CalculateTransformFromAToB(IList<Vector3> pointsInA,
            IList<Vector3> pointsInB)
        {
            if (pointsInA.Count != pointsInB.Count || pointsInA.Count != 3)
            {
                return (Matrix4x4.identity, Mathf.Infinity);
            }

            var aPPointsCenter = (pointsInA[0] + pointsInA[1] + pointsInA[2]) / 3.0f;
            var bPPointsCenter = (pointsInB[0] + pointsInB[1] + pointsInB[2]) / 3.0f;
            var aTPoints = Matrix4x4.TRS(aPPointsCenter,
                Quaternion.LookRotation(pointsInA[0] - pointsInA[2], pointsInA[0] - pointsInA[1]), Vector3.one);
            var bTPoints = Matrix4x4.TRS(bPPointsCenter,
                Quaternion.LookRotation(pointsInB[0] - pointsInB[2], pointsInB[0] - pointsInB[1]), Vector3.one);
            var bTA = bTPoints * Matrix4x4.Inverse(aTPoints);

            var matchingDeviation = 0.0f;
            for (var i = 0; i < pointsInA.Count; ++i)
                matchingDeviation += (pointsInB[i] - bTA.MultiplyPoint(pointsInA[i])).magnitude;

            return (bTA, matchingDeviation / pointsInA.Count);
        }
    }

    public class CalibrationData
    {
        public Marker Marker1;
        public Marker Marker2;
        public Marker Marker3;

        public Vector3[] PointsInLocationOrigin;
        //    { new(10.615f, 6.802f, 2.355f), new(8.448f, 6.809f, 4.812f), new(8.734f, 9.936f, 2.708f) };

        public Vector3 Offset { get; set; }

        public CalibrationData(string name, Vector3[] pointsInLocationOrigin)
        {
            Name = name;
            PointsInLocationOrigin = pointsInLocationOrigin;
        }

        public string Name { get; private set; }
        public bool IsValid => Marker.IsValid(Marker1) && Marker.IsValid(Marker2) && Marker.IsValid(Marker3);
        public int NumberOfReferencePoints => 3;

        public void UpdateMarkers(ARAnchor anchor1, ARAnchor anchor2, ARAnchor anchor3)
        {
            Marker1 = new Marker(anchor1);
            Marker2 = new Marker(anchor2);
            Marker3 = new Marker(anchor3);
        }
    }


    public class Marker
    {
        private readonly ARAnchor _anchor;

        public Marker(ARAnchor anchor)
        {
            _anchor = anchor;
        }

        public Vector3 Position => _anchor ? _anchor.transform.position : Vector3.zero;

        public static bool IsValid(Marker marker)
        {
            return marker != null && marker._anchor != null;
        }
    }
}
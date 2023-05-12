using System;
using System.Collections.Generic;
using System.IO;
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
                calibrationData.Marker1.GetUpdatedPosition(),
                calibrationData.Marker2.GetUpdatedPosition(),
                calibrationData.Marker3.GetUpdatedPosition()
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

    [Serializable]
    public class CalibrationData
    {
        public Marker Marker1;
        public Marker Marker2;
        public Marker Marker3;

        public Vector3[] PointsInLocationOrigin;

        public string Name;
        public Vector3 Offset;

        public CalibrationData(string name, Vector3[] pointsInLocationOrigin)
        {
            Name = name;
            PointsInLocationOrigin = pointsInLocationOrigin;
        }

        public bool IsValid => Marker.IsValid(Marker1) && Marker.IsValid(Marker2) && Marker.IsValid(Marker3);
        public int NumberOfReferencePoints => 3;

        public static CalibrationData TryLoad(string name)
        {
            var filePath = Path.Combine(Application.persistentDataPath, name, "calibrationdata.json");
            Debug.Log(filePath);
            try
            {
                var reader = new StreamReader(filePath);
                return JsonUtility.FromJson<CalibrationData>(reader.ReadToEnd());
            }
            catch (Exception)
            {
                Debug.LogError($"Failed to read file at {filePath}");
            }

            return null;
        }

        public void Store(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);

            Marker1.UpdatePosition();
            Marker2.UpdatePosition();
            Marker3.UpdatePosition();

            var jsonContent = JsonUtility.ToJson(this);
            Debug.Log($"calibration serialized content: {jsonContent}");

            var filePath = Path.Combine(directoryPath, "calibrationdata.json");
            var writer = new StreamWriter(filePath);
            writer.Write(jsonContent);
            writer.Close();

            Debug.Log($"calibration data store to file {filePath}");
        }

        public void UpdateMarkers(ARAnchor anchor1, ARAnchor anchor2, ARAnchor anchor3)
        {
            Marker1 = new Marker(anchor1);
            Marker2 = new Marker(anchor2);
            Marker3 = new Marker(anchor3);
        }
    }


    [Serializable]
    public class Marker
    {
        public Vector3 Position;
        private readonly ARAnchor _anchor;

        public Marker(ARAnchor anchor)
        {
            _anchor = anchor;
            Position = _anchor ? _anchor.transform.position : Vector3.zero;
        }

        public Vector3 GetUpdatedPosition()
        {
            UpdatePosition();
            return Position;
        }

        public void UpdatePosition()
        {
            Position = _anchor ? _anchor.transform.position : Vector3.zero;
        }

        public static bool IsValid(Marker marker)
        {
            return marker != null && marker._anchor != null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                calibrationData.Markers[0].Position,
                calibrationData.Markers[1].Position,
                calibrationData.Markers[2].Position
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
    public struct _CalibrationData
    {
        public string Name;
        public Vector3[] PointsInWorldMap;
        public Vector3[] PointsInLocationOrigin;
        public Vector3 Offset;

        public _CalibrationData(CalibrationData calibrationData)
        {
            Name = calibrationData.Name;
            var pointsInWorldMap = new List<Vector3>();
            foreach (var marker in calibrationData.Markers)
                pointsInWorldMap.Add(marker.Position);
            PointsInWorldMap = pointsInWorldMap.ToArray();
            PointsInLocationOrigin = calibrationData.PointsInLocationOrigin;
            Offset = calibrationData.Offset;
        }
    }

    public class CalibrationData
    {
        public string Name;
        public List<Marker> Markers;
        public Vector3[] PointsInLocationOrigin;
        public Vector3 Offset;

        public CalibrationData(string name, Vector3[] pointsInLocationOrigin)
        {
            Name = name;
            PointsInLocationOrigin = pointsInLocationOrigin;
            Offset = Vector3.zero;

            var placedMarker = MonoBehaviour.FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();
            placedMarker.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name, StringComparison.Ordinal));

            Markers = new List<Marker>();
            foreach (var markerObj in placedMarker)
                Markers.Add(new Marker(markerObj.gameObject));
        }

        public CalibrationData(_CalibrationData data)
        {
            Name = data.Name;
            PointsInLocationOrigin = data.PointsInLocationOrigin;
            Offset = data.Offset;

            Markers = new List<Marker>();
            if (data.PointsInWorldMap.Length > 0)
            {
                var anchorsInWorldMap = MonoBehaviour.FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();

                foreach (var storedPointInWorldMap in data.PointsInWorldMap)
                {
                    foreach (var anchorInWorldMap in anchorsInWorldMap)
                    {
                        if ((storedPointInWorldMap - anchorInWorldMap.transform.position).magnitude < 0.1)
                            Markers.Add(new Marker(anchorInWorldMap.gameObject));
                    }
                }
            }
        }

        public bool IsValid => Markers.Count == 3 && Marker.IsValid(Markers[0]) && Marker.IsValid(Markers[1]) && Marker.IsValid(Markers[2]);

        public int NumberOfReferencePoints => PointsInLocationOrigin.Length;

        public static CalibrationData TryLoad(string name)
        {
            var filePath = Path.Combine(Application.persistentDataPath, name, "calibrationdata.json");
            Debug.Log(filePath);
            try
            {
                var reader = new StreamReader(filePath);
                return new CalibrationData(JsonUtility.FromJson<_CalibrationData>(reader.ReadToEnd()));
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

            var data = new _CalibrationData(this);
            var jsonContent = JsonUtility.ToJson(data);
            Debug.Log($"calibration serialized content: {jsonContent}");

            var filePath = Path.Combine(directoryPath, "calibrationdata.json");
            var writer = new StreamWriter(filePath);
            writer.Write(jsonContent);
            writer.Close();

            Debug.Log($"calibration data store to file {filePath}");
        }

        //public void UpdateMarkers(ARAnchor anchor1, ARAnchor anchor2, ARAnchor anchor3)
        //{
        //    Marker1 = new Marker(anchor1);
        //    Marker2 = new Marker(anchor2);
        //    Marker3 = new Marker(anchor3);
        //}
    }


    public class Marker
    {
        public Vector3 Position
        {
            get
            {
                return _anchor ? _anchor.transform.position : Vector3.zero;
            }
        }

        public GameObject GameObject { get { return _markerObj; } }

        private readonly ARAnchor _anchor;
        private readonly GameObject _markerObj;

        public Marker(GameObject markerObj)//ARAnchor anchor)
        {
            _markerObj = markerObj;

            _anchor = markerObj.GetComponent<ARAnchor>();
        }

        public static bool IsValid(Marker marker)
        {
            return marker != null && marker._anchor != null;
        }
    }
}
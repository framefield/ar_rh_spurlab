using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;

namespace ff.ar_rh_spurlab.Calibration
{
    public static class CalibrationCalculator
    {
        public static (Matrix4x4 xrOriginTLocationOrigin, bool isValid) GetXrOriginTLocationOrigin(
            CalibrationData calibrationData, LocationData locationData)
        {
            if (calibrationData?.IsValid != true)
            {
                return (Matrix4x4.identity, false);
            }

            calibrationData.Update();

            var (xrOriginTLocationOrigin, matchingDeviation) =
                CalculateTransformFromAToB(locationData._pointsInLocationOrigin, calibrationData.PointsInWorldMap);

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
        public string Name;
        public Vector3 Offset;
        public Vector3[] PointsInWorldMap;
        public List<Marker> Markers = new();

        public CalibrationData(string name)
        {
            Name = name;
            Offset = Vector3.zero;

            PointsInWorldMap = new Vector3[LocationData.NumberOfReferencePoints];
        }

        public bool IsValid => Markers != null && Markers.Count == LocationData.NumberOfReferencePoints &&
                               Markers.All(Marker.IsValid);

        public void SearchForMarkers()
        {
            Markers = new List<Marker>();
            if (PointsInWorldMap.Length == 0)
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            var anchorsInWorldMap = UnityEngine.Object.FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();

            foreach (var storedPointInWorldMap in PointsInWorldMap)
            {
                foreach (var anchorInWorldMap in anchorsInWorldMap)
                {
                    var isSamePoint = (storedPointInWorldMap - anchorInWorldMap.transform.position).magnitude < 0.1;
                    if (isSamePoint)
                    {
                        Markers.Add(new Marker(anchorInWorldMap));
                        break;
                    }
                }
            }
            
            Debug.Log($"Found {anchorsInWorldMap.Count} anchors in world map and matched {Markers.Count} of them.");
#else
            for (var i = 0; i < PointsInWorldMap.Length; i++)
            {
                var anchorGameObject = new GameObject
                {
                    name = $"ARMarkerAnchor_{i + 1}",
                    transform =
                    {
                        position = PointsInWorldMap[i]
                    }
                };
                var anchor = anchorGameObject.AddComponent<ARAnchor>();
                Markers.Add(new Marker(anchor));
            }
#endif
        }

        public void Update()
        {
            for (var i = 0; i < Mathf.Min(Markers.Count, PointsInWorldMap.Length); ++i)
                PointsInWorldMap[i] = Markers[i].Position;
        }

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
            var jsonContent = JsonUtility.ToJson(this);
            Debug.Log($"calibration serialized content: {jsonContent}");

            var filePath = Path.Combine(directoryPath, "calibrationdata.json");
            var writer = new StreamWriter(filePath);
            writer.Write(jsonContent);
            writer.Close();

            Debug.Log($"calibration data store to file {filePath}");
        }

        public void Reset()
        {
            foreach (var marker in Markers)
            {
                Object.Destroy(marker.GameObject);
            }

            Markers.Clear();
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
        public GameObject GameObject => _anchor ? _anchor.gameObject : null;

        public static bool IsValid(Marker marker)
        {
            return marker != null && marker._anchor != null;
        }
    }
}
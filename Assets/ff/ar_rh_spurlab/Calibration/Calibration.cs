using System;
using System.Collections.Generic;
using System.IO;
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
            {
                matchingDeviation += (pointsInB[i] - bTA.MultiplyPoint(pointsInA[i])).magnitude;
            }

            return (bTA, matchingDeviation / pointsInA.Count);
        }
    }

    [Serializable]
    public class CalibrationData
    {
        public string Name;
        public Vector3 Offset;
        public Vector3[] PointsInWorldMap;

        [NonSerialized]
        public bool AreAnchorsReady;

        public CalibrationData(string name)
        {
            Name = name;
            Offset = Vector3.zero;
            PointsInWorldMap = new Vector3[LocationData.NumberOfReferencePoints];
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
    }

    public class CalibrationARAnchorManager
    {
        public enum Mode
        {
            Calibration,
            Tracking
        }

        private readonly List<ARAnchor> _allAnchors = new();


        private readonly ARAnchorManager _arAnchorManager;

        private readonly List<ARAnchor> _matchedAnchors = new();
        private readonly Mode _mode;
        private CalibrationData _calibrationData;

        public CalibrationARAnchorManager(ARAnchorManager arAnchorManager, Mode mode)
        {
            _arAnchorManager = arAnchorManager;
            _mode = mode;

            _arAnchorManager.anchorsChanged += OnAnchorsChanged;
        }

        private bool AreAnchorsReady => _matchedAnchors.Count == LocationData.NumberOfReferencePoints;

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
        {
            Debug.Log("On Anchors Changed");

            switch (_mode)
            {
                case Mode.Calibration:
                    UpdateAnchorsInCalibration(args);
                    break;
                case Mode.Tracking:
                    UpdateAnchorsInTracking(args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _calibrationData.AreAnchorsReady = AreAnchorsReady;

            for (var i = 0; i < _matchedAnchors.Count; i++)
            {
                _calibrationData.PointsInWorldMap[i] = _matchedAnchors[i].transform.position;
            }
        }

        private void UpdateAnchorsInCalibration(ARAnchorsChangedEventArgs args)
        {
            foreach (var removedAnchor in args.removed)
            {
                _matchedAnchors.Remove(removedAnchor);
            }

            foreach (var addedAnchor in args.added)
            {
                if (_matchedAnchors.Count < LocationData.NumberOfReferencePoints)
                {
                    _matchedAnchors.Add(addedAnchor);
                }
            }

            if (_calibrationData == null)
            {
                return;
            }

            for (var i = 0; i < _matchedAnchors.Count; i++)
            {
                _calibrationData.PointsInWorldMap[i] = _matchedAnchors[i].transform.position;
            }
        }

        private void UpdateAnchorsInTracking(ARAnchorsChangedEventArgs args)
        {
            foreach (var removedAnchor in args.removed)
            {
                _allAnchors.Remove(removedAnchor);
            }

            foreach (var addedAnchor in args.added)
            {
                _allAnchors.Add(addedAnchor);
            }

            if (args.removed.Count != 0 || args.added.Count != 0)
            {
                MatchExistingAnchors();
            }
        }

        private void MatchExistingAnchors()
        {
            _matchedAnchors.Clear();

            if (_calibrationData == null || _allAnchors.Count < _calibrationData.PointsInWorldMap.Length)
            {
                return;
            }

            foreach (var storedPointInWorldMap in _calibrationData.PointsInWorldMap)
            {
                foreach (var anchor in _allAnchors)
                {
                    if (Vector3.Distance(anchor.transform.position, storedPointInWorldMap) < 0.1f)
                    {
                        _matchedAnchors.Add(anchor);
                        break;
                    }
                }
            }
        }

        public void SetCalibrationData(CalibrationData calibrationData)
        {
            _calibrationData = calibrationData;
            MatchExistingAnchors();
        }

        public void Reset()
        {
            if (_mode != Mode.Calibration)
            {
                return;
            }

            foreach (var anchor in _allAnchors)
            {
                Object.Destroy(anchor.gameObject);
            }

            _allAnchors.Clear();
            _matchedAnchors.Clear();
        }
    }
}
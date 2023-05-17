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

            var allowedMatchingDeviation = 0.5f;
#if UNITY_EDITOR
            allowedMatchingDeviation = 100.0f; //for testing any quality is allowed
#endif

            return (calibrationData.Offset * xrOriginTLocationOrigin, matchingDeviation < allowedMatchingDeviation);
        }

        private static (Matrix4x4, float) CalculateTransformFromAToB(IList<Vector3> pointsInA,
            IList<Vector3> pointsInB)
        {
            if (pointsInA.Count != pointsInB.Count || pointsInA.Count != 3)
            {
                Debug.LogError("failed to calculate transform: number of points does not match");
                return (Matrix4x4.identity, Mathf.Infinity);
            }

            if (Vector3.Distance(pointsInA[0], pointsInA[1]) < 0.1f ||
                Vector3.Distance(pointsInA[0], pointsInA[2]) < 0.1f ||
                Vector3.Distance(pointsInB[0], pointsInB[1]) < 0.1f ||
                Vector3.Distance(pointsInB[0], pointsInB[2]) < 0.1f)
            {
                Debug.LogError("failed to calculate transform: points are too close to each other");
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
                matchingDeviation += Vector3.Distance(pointsInB[i], bTA.MultiplyPoint(pointsInA[i]));
            }

            return (bTA, matchingDeviation / pointsInA.Count);
        }
    }

    [Serializable]
    public class CalibrationData
    {
        public string Name;
        public Matrix4x4 Offset;
        public List<Vector3> PointsInWorldMap;

        // matched anchors are stored here to be able to simulate anchors in editor.
        [NonSerialized]
        public List<ARAnchor> MatchedAnchors = new();

        public CalibrationData(string name)
        {
            Name = name;
            Offset = Matrix4x4.identity;
            PointsInWorldMap = new List<Vector3>();
        }

        public bool AreAnchorsReady => MatchedAnchors.Count == LocationData.NumberOfReferencePoints;

        public void UpdatePointsFromsAnchors()
        {
            PointsInWorldMap.Clear();
            for (var i = 0; i < MatchedAnchors.Count; i++)
            {
                PointsInWorldMap.Add(MatchedAnchors[i].transform.position);
            }
        }

        public static CalibrationData TryLoad(string name)
        {
            var filePath = Path.Combine(Application.persistentDataPath, name, "calibrationdata.json");
            Debug.Log(filePath);
            try
            {
                var reader = new StreamReader(filePath);
                var readCalibrationData = JsonUtility.FromJson<CalibrationData>(reader.ReadToEnd());
                readCalibrationData.MatchedAnchors = new List<ARAnchor>();
                return readCalibrationData;
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
            Calibrating,
            Tracking
        }

        private readonly List<ARAnchor> _allAnchors = new();


        private readonly ARAnchorManager _arAnchorManager;

        private readonly Mode _mode;
        private CalibrationData _calibrationData;

        public CalibrationARAnchorManager(ARAnchorManager arAnchorManager, Mode mode)
        {
            _arAnchorManager = arAnchorManager;
            _mode = mode;

            _arAnchorManager.anchorsChanged += OnAnchorsChanged;
        }

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
        {
            Debug.Log("On Anchors Changed");

            switch (_mode)
            {
                case Mode.Calibrating:
                    UpdateAnchorsInCalibration(args);
                    break;
                case Mode.Tracking:
                    UpdateAnchorsInTracking(args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _calibrationData.UpdatePointsFromsAnchors();
        }

        private void UpdateAnchorsInCalibration(ARAnchorsChangedEventArgs args)
        {
            // do nothing, PointSelectionController handles the anchors.
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
            if (_mode == Mode.Calibrating)
            {
                return;
            }

            if (_calibrationData == null)
            {
                return;
            }

            _calibrationData.MatchedAnchors ??= new List<ARAnchor>();
            _calibrationData.MatchedAnchors.Clear();

#if UNITY_IOS && !UNITY_EDITOR
            if (_allAnchors.Count < _calibrationData.PointsInWorldMap.Count)
            {
                return;
            }

            for (var i = 0; i < _calibrationData.PointsInWorldMap.Count; i++)
            {
                foreach (var anchor in _allAnchors)
                {
                    if (Vector3.Distance(anchor.transform.position, _calibrationData.PointsInWorldMap[i]) < 0.1f)
                    {
                        _calibrationData.MatchedAnchors.Add(anchor);
                        _calibrationData.PointsInWorldMap[i] = anchor.transform.position;
                        break;
                    }
                }
            }
#elif UNITY_EDITOR
            // simulate anchors in editor
            foreach (var position in _calibrationData.PointsInWorldMap)
            {
                var gameObject = Object.Instantiate(_arAnchorManager.anchorPrefab);
                gameObject.transform.position = position;
                var anchor = gameObject.GetComponent<ARAnchor>();
                _calibrationData.MatchedAnchors.Add(anchor);
            }
#endif
        }

        public void SetCalibrationData(CalibrationData calibrationData)
        {
            _calibrationData = calibrationData;
            MatchExistingAnchors();
        }

        public void Reset()
        {
            if (_mode != Mode.Calibrating)
            {
                return;
            }

            foreach (var anchor in _allAnchors)
            {
                Object.Destroy(anchor.gameObject);
            }

            _allAnchors.Clear();

            _calibrationData?.MatchedAnchors.Clear();
            _calibrationData?.UpdatePointsFromsAnchors();
        }
    }
}
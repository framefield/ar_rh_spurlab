using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ff.ar_rh_spurlab.Locations;
using ff.common.entity;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ff.ar_rh_spurlab.Calibration
{
    public static class CalibrationCalculator
    {
#if UNITY_EDITOR
        //for testing any quality is allowed
        const float allowedMatchingDeviation = 100.0f;
        const float percentageDeviation = 0.5f;
#else
        const float allowedMatchingDeviation = 0.5f;
        const float percentageDeviation = allowedMatchingDeviation;
#endif

        public static string GetCalibrationMessage(CalibrationData calibrationData, LocationData locationData)
        {
            if (calibrationData == null || locationData == null)
            {
                return "No calibration data available";
            }

            if (!calibrationData.AreAnchorsReady)
            {
                var sb = new StringBuilder();
                sb.Append(
                    $"Missing anchors ({calibrationData.MatchedAnchorsCount}/{calibrationData.MatchedAnchors.Count}): ");
                for (var index = 0; index < calibrationData.MatchedAnchors.Count; index++)
                {
                    var arAnchor = calibrationData.MatchedAnchors[index];
                    var pointInformation = locationData.PointsInformation[index];
                    if (arAnchor == null)
                    {
                        sb.Append(
                            $"#{index} ({pointInformation.Title.Value(ApplicationLocale.Instance.CurrentLocale)}) ");
                    }
                }

                if (calibrationData.UnmatchedAnchors?.Count > 0)
                {
                    sb.Append($" [{calibrationData.UnmatchedAnchors.Count} unmatched]");
                }

                return sb.ToString();
            }


            return string.Empty;
        }

        public static (Matrix4x4 xrOriginTLocationOrigin, bool isValid, float quality) GetXrOriginTLocationOrigin(
            CalibrationData calibrationData, LocationData locationData)
        {
            var (xrOriginTLocationOrigin, matchingDeviation) =
                CalculateTransformFromAToB(locationData.PointsInLocationOrigin, calibrationData.PointsInWorldMap);

            var qualityLevelPercentage = Mathf.Clamp01(1.0f - matchingDeviation / percentageDeviation);
            return (calibrationData.Offset * xrOriginTLocationOrigin, matchingDeviation < allowedMatchingDeviation,
                qualityLevelPercentage);
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
                Quaternion.LookRotation(pointsInA[0] - pointsInA[2], pointsInA[0] - pointsInA[1]),
                Vector3.one);
            var bTPoints = Matrix4x4.TRS(bPPointsCenter,
                Quaternion.LookRotation(pointsInB[0] - pointsInB[2], pointsInB[0] - pointsInB[1]),
                Vector3.one);
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
        public string Id;

        [SerializeField]
        private string Name;

        public Matrix4x4 Offset;
        public List<Vector3> PointsInWorldMap;

        public IReadOnlyList<ARAnchor> MatchedAnchors => _matchedAnchors;
        public IReadOnlyList<ARAnchor> UnmatchedAnchors => _unmatchedAnchors;

        // matched anchors are stored here to be able to simulate anchors in editor.
        [NonSerialized]
        internal List<ARAnchor> _matchedAnchors;

        [NonSerialized]
        internal List<ARAnchor> _unmatchedAnchors;

        public bool AreAnchorsReady => MatchedAnchorsCount == LocationData.NumberOfReferencePoints;
        public int MatchedAnchorsCount => _matchedAnchors.Count(a => a != null);

        public float Quality => LocationData.NumberOfReferencePoints > 0
            ? (float)MatchedAnchorsCount / LocationData.NumberOfReferencePoints
            : 0.0f;

        public CalibrationData(string id)
        {
            Id = id;
            Offset = Matrix4x4.identity;
            PointsInWorldMap = new List<Vector3>();
            _matchedAnchors = new List<ARAnchor>();
            _unmatchedAnchors = new List<ARAnchor>();
        }

        public void UpdatePointsFromAnchors()
        {
            PointsInWorldMap.Clear();
            foreach (var t in _matchedAnchors)
            {
                PointsInWorldMap.Add(t.transform.position);
            }
        }

        public static bool CalibrationDataExists(string id)
        {
            var filePath = Path.Combine(Application.persistentDataPath, id, "calibrationdata.json");
            return File.Exists(filePath);
        }

        public static CalibrationData TryLoad(string id)
        {
            var filePath = Path.Combine(Application.persistentDataPath, id, "calibrationdata.json");
            // Debug.Log(filePath);
            try
            {
                var reader = new StreamReader(filePath);
                var readCalibrationData = JsonUtility.FromJson<CalibrationData>(reader.ReadToEnd());
                // JSONUtility does not initialize non-serialized fields
                readCalibrationData._matchedAnchors = new List<ARAnchor>();
                readCalibrationData._unmatchedAnchors = new List<ARAnchor>();
                readCalibrationData.Id ??= readCalibrationData.Name;
                readCalibrationData.ClearInstantiatedAnchors();
                return readCalibrationData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read file at {filePath}");
                Debug.LogException(e);
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

        public void AddInstantiatedAnchor(ARAnchor marker)
        {
            _matchedAnchors.Add(marker);
        }

        public void ClearInstantiatedAnchors()
        {
            foreach (var placedAnchor in _matchedAnchors)
            {
                if (placedAnchor || placedAnchor.gameObject)
                {
                    Object.Destroy(placedAnchor.gameObject);
                }
            }

            _matchedAnchors.Clear();
        }
    }

    public class CalibrationARAnchorManager
    {
        const float anchorMatchingDistance = 1.0f;

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

            _calibrationData.UpdatePointsFromAnchors();
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

            if (args.removed.Count != 0 || args.added.Count != 0 || args.updated.Count != 0)
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

            _calibrationData._matchedAnchors.Clear();
            _calibrationData._unmatchedAnchors.Clear();

#if UNITY_IOS && !UNITY_EDITOR
            var availableAnchors = new List<ARAnchor>(_allAnchors);
            for (var i = 0; i < _calibrationData.PointsInWorldMap.Count; i++)
            {
                ARAnchor foundAnchor = null;
                foreach (var anchor in availableAnchors)
                {
                    if (Vector3.Distance(anchor.transform.position, _calibrationData.PointsInWorldMap[i]) < anchorMatchingDistance)
                    {
                        _calibrationData.PointsInWorldMap[i] = anchor.transform.position;
                        availableAnchors.Remove(anchor);
                        foundAnchor = anchor;
                        break;
                    }
                }
                _calibrationData._matchedAnchors.Add(foundAnchor);
            }
            _calibrationData._unmatchedAnchors = availableAnchors;
#elif UNITY_EDITOR
            // simulate anchors in editor
            for (var index = 0; index < _calibrationData.PointsInWorldMap.Count; index++)
            {
                var position = _calibrationData.PointsInWorldMap[index];
                if (_simulateJitterCalibration)
                {
                    position += Random.insideUnitSphere * 0.25f;
                }

                var gameObject = Object.Instantiate(_arAnchorManager.anchorPrefab);
                gameObject.transform.position = position;
                var anchor = gameObject.GetComponent<ARAnchor>();
                _allAnchors.Add(anchor);

                if (_simulateMissingPointFromCalibration && index == 1)
                {
                    gameObject.transform.localScale *= 0.2f;
                    _calibrationData._matchedAnchors.Add(null);
                    _calibrationData._unmatchedAnchors.Add(anchor);
                }
                else
                {
                    _calibrationData._matchedAnchors.Add(anchor);
                }
            }
#endif
        }

        public void SetCalibrationData(CalibrationData calibrationData)
        {
            if (calibrationData != _calibrationData)
            {
                foreach (var anchor in _allAnchors)
                {
                    Object.Destroy(anchor.gameObject);
                }

                _allAnchors.Clear();

                _calibrationData = calibrationData;
                MatchExistingAnchors();
            }
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
            _calibrationData?._matchedAnchors.Clear();
            _calibrationData?.UpdatePointsFromAnchors();
        }

#if UNITY_EDITOR
        private bool _simulateMissingPointFromCalibration = false;
        private bool _simulateJitterCalibration = false;

        public void ToggleMissingPointFromCalibration()
        {
            _simulateMissingPointFromCalibration = !_simulateMissingPointFromCalibration;
            foreach (var anchor in _allAnchors)
            {
                Object.Destroy(anchor.gameObject);
            }

            _allAnchors.Clear();
            MatchExistingAnchors();
        }


        public void ToggleJitterCalibration()
        {
            _simulateJitterCalibration = !_simulateJitterCalibration;
            foreach (var anchor in _allAnchors)
            {
                Object.Destroy(anchor.gameObject);
            }

            _allAnchors.Clear();
            MatchExistingAnchors();
            _calibrationData.UpdatePointsFromAnchors();
        }
#endif
    }
}

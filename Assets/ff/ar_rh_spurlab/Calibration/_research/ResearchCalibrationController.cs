using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ff.ar_rh_spurlab.Calibration;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

namespace ff.ar_rh_spurlab
{
    /// <summary>
    ///     Listens for touch events and performs an AR raycast from the screen touch point.
    ///     AR raycasts will only hit detected trackables like feature points and planes.
    ///     If a raycast hits a trackable, the <see cref="placedPrefab" /> is instantiated
    ///     and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ResearchCalibrationController : PressInputBase
    {
        private const int MAX_NUMBER_OF_REFERENCE_POINTS = 3;

        private static readonly List<ARRaycastHit> s_Hits = new();

        [SerializeField]
        private ARSession m_ARSession;

        [SerializeField]
        private GameObject m_MarkerPrefab;

        [SerializeField]
        private Transform m_XROrigin;

        [SerializeField]
        private ARAnchorManager _arAnchorManager;

        [SerializeField]
        private GameObject m_LocationPrefab;

        [Tooltip("UI Text component to display error messages")]
        [SerializeField]
        private Text m_ErrorText;

        [Tooltip("The UI Text element used to display log messages.")]
        [SerializeField]
        private Text m_LogText;

        [Tooltip("The UI Text element used to display the current AR world mapping status.")]
        [SerializeField]
        private Text m_MappingStatusText;

        [Tooltip("A UI button component which will generate an ARWorldMap and save it to disk.")]
        [SerializeField]
        private Button m_SaveButton;

        [Tooltip(
            "A UI button component which will load a previously saved ARWorldMap from disk and apply it to the current session.")]
        [SerializeField]
        private Button m_LoadButton;

        [SerializeField]
        private Button m_ResetButton;

        private CalibrationAnchorController _calibrationAnchorController;
        private GameObject m_LocationObject;

        private List<string> m_LogMessages;
        private bool m_Pressed;
        private ARRaycastManager m_RaycastManager;

        private static string path => Path.Combine(Application.persistentDataPath, "my_session.worldmap");

        private bool supported
        {
            get
            {
#if UNITY_IOS
                return m_ARSession.subsystem is ARKitSessionSubsystem && ARKitSessionSubsystem.worldMapSupported;
#else
                return false;
#endif
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_RaycastManager = GetComponent<ARRaycastManager>();
            m_LogMessages = new List<string>();
            _calibrationAnchorController = new CalibrationAnchorController(_arAnchorManager);
        }

        private void Update()
        {
            _calibrationAnchorController.Update();

            if (supported)
            {
                SetActive(m_ErrorText, false);
                SetActive(m_SaveButton, true);
                SetActive(m_LoadButton, true);
                SetActive(m_MappingStatusText, true);
            }
            else
            {
                SetActive(m_ErrorText, true);
                SetActive(m_SaveButton, false);
                SetActive(m_LoadButton, false);
                SetActive(m_MappingStatusText, false);
            }

#if UNITY_IOS
            ARKitSessionSubsystem sessionSubsystem = null;
            try
            {
                sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            }
            catch
            {
                sessionSubsystem = null;
            }

            if (sessionSubsystem != null)
            {
                var numLogsToShow = 20;
                var msg = "";
                for (var i = Mathf.Max(0, m_LogMessages.Count - numLogsToShow); i < m_LogMessages.Count; ++i)
                {
                    msg += m_LogMessages[i];
                    msg += "\n";
                }

                SetText(m_LogText, msg);
                SetText(m_MappingStatusText, $"Mapping Status: {sessionSubsystem.worldMappingStatus}");
            }
#endif

            var placedMarker = FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();
            placedMarker.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));
            Log($"current num marker {placedMarker.Count}");

            if (Pointer.current != null && m_Pressed)
            {
                var touchPosition = Pointer.current.position.ReadValue();

                if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycast hits are sorted by distance, so the first one will be the closest hit.
                    var hitPose = s_Hits[0].pose;

                    GameObject selectedMarkerObject = null;
                    foreach (var obj in placedMarker)
                        if ((obj.transform.position - hitPose.position).magnitude < 0.1)
                        {
                            selectedMarkerObject = obj.gameObject;
                            break;
                        }

                    if (selectedMarkerObject != null)
                    {
                        selectedMarkerObject.transform.position = hitPose.position;
                    }
                    else
                    {
                        if (placedMarker.Count < MAX_NUMBER_OF_REFERENCE_POINTS)
                        {
                            var marker = Instantiate(m_MarkerPrefab, hitPose.position, hitPose.rotation, m_XROrigin);
                            var arAnchor = marker.GetComponent<ARAnchor>();
                            var calibrationId = $"ARMarkerAnchor_{placedMarker.Count}";
                            marker.name = calibrationId;
                            placedMarker.Add(arAnchor);
                            _calibrationAnchorController.TryAddCalibrationId(calibrationId, arAnchor);
                        }
                    }
                }
            }

#if UNITY_IOS
            if (sessionSubsystem != null)
            {
                var isTrackingStable = sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Mapped ||
                                       sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Extending;
                if (m_LocationObject != null)
                {
                    m_LocationObject.SetActive(isTrackingStable);
                }
            }
#endif

            if (placedMarker.Count == MAX_NUMBER_OF_REFERENCE_POINTS)
            {
                Vector3[] pointsInLocationOrigin =
                    { new(10.615f, 6.802f, 2.355f), new(8.448f, 6.809f, 4.812f), new(8.734f, 9.936f, 2.708f) };
                Vector3[] pointsInXROrigin =
                {
                    placedMarker[0].gameObject.transform.position,
                    placedMarker[1].gameObject.transform.position,
                    placedMarker[2].gameObject.transform.position
                };

                var (xrOriginTLocationOrigin, matchingDeviation) =
                    calculateTransfromFromeAToB(pointsInLocationOrigin, pointsInXROrigin);
                if (m_LocationObject == null)
                {
                    m_LocationObject = Instantiate(m_LocationPrefab, xrOriginTLocationOrigin.GetPosition(),
                        xrOriginTLocationOrigin.rotation, m_XROrigin);
                    Log($"matching deviation: {matchingDeviation}");
                }
                else
                {
                    m_LocationObject.transform.position = xrOriginTLocationOrigin.GetPosition();
                    m_LocationObject.transform.rotation = xrOriginTLocationOrigin.rotation;
                }
            }
        }

        /// <summary>
        ///     Create an <c>ARWorldMap</c> and save it to disk.
        /// </summary>
        public void OnSaveButton()
        {
#if UNITY_IOS
            StartCoroutine(Save());
#endif
        }

        /// <summary>
        ///     Load an <c>ARWorldMap</c> from disk and apply it to the current session.
        /// </summary>
        public void OnLoadButton()
        {
#if UNITY_IOS
            StartCoroutine(Load());
#endif
        }

        /// <summary>
        ///     Reset the <c>ARSession</c>, destroying any existing trackables, such as planes.
        ///     Upon loading a saved <c>ARWorldMap</c>, saved trackables will be restored.
        /// </summary>
        public void OnResetButton()
        {
            m_ARSession.Reset();
        }

        private void Log(string logMessage)
        {
            m_LogMessages.Add(logMessage);
        }

        private static void SetActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        private static void SetActive(Text text, bool active)
        {
            if (text != null)
            {
                text.gameObject.SetActive(active);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static (Matrix4x4, float) calculateTransfromFromeAToB(IList<Vector3> pointsInA,
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

        protected override void OnPress(Vector3 position)
        {
            m_Pressed = true;
        }

        protected override void OnPressCancel()
        {
            m_Pressed = false;
        }

#if UNITY_IOS
        private IEnumerator Save()
        {
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            if (sessionSubsystem == null)
            {
                Log("No session subsystem available. Could not save.");
                yield break;
            }

            var request = sessionSubsystem.GetARWorldMapAsync();

            while (!request.status.IsDone())
                yield return null;

            if (request.status.IsError())
            {
                Log($"Session serialization failed with status {request.status}");
                yield break;
            }

            var worldMap = request.GetWorldMap();
            request.Dispose();

            SaveAndDisposeWorldMap(worldMap);
        }

        private void SaveAndDisposeWorldMap(ARWorldMap worldMap)
        {
            Log("Serializing ARWorldMap to byte array...");
            var data = worldMap.Serialize(Allocator.Temp);
            Log($"ARWorldMap has {data.Length} bytes.");

            var file = File.Open(path, FileMode.Create);
            var writer = new BinaryWriter(file);
            writer.Write(data.ToArray());
            writer.Close();
            data.Dispose();
            worldMap.Dispose();
            Log($"ARWorldMap written to {path}");
        }

        private IEnumerator Load()
        {
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            if (sessionSubsystem == null)
            {
                Log("No session subsystem available. Could not load.");
                yield break;
            }

            FileStream file;
            try
            {
                file = File.Open(path, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                Debug.LogError(
                    "No ARWorldMap was found. Make sure to save the ARWorldMap before attempting to load it.");
                yield break;
            }

            Log($"Reading {path}...");

            const int bytesPerFrame = 1024 * 10;
            var bytesRemaining = file.Length;
            var binaryReader = new BinaryReader(file);
            var allBytes = new List<byte>();
            while (bytesRemaining > 0)
            {
                var bytes = binaryReader.ReadBytes(bytesPerFrame);
                allBytes.AddRange(bytes);
                bytesRemaining -= bytesPerFrame;
                yield return null;
            }

            var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
            data.CopyFrom(allBytes.ToArray());

            Log("Deserializing to ARWorldMap...");
            if (ARWorldMap.TryDeserialize(data, out var worldMap))
            {
                data.Dispose();
            }

            if (worldMap.valid)
            {
                Log("Deserialized successfully.");
            }
            else
            {
                Debug.LogError("Data is not a valid ARWorldMap.");
                yield break;
            }

            Log("Apply ARWorldMap to current session.");
            sessionSubsystem.ApplyWorldMap(worldMap);
        }
#endif
    }
}
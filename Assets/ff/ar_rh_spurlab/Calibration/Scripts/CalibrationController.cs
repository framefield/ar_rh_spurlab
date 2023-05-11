using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen touch point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class CalibrationController : PressInputBase
    {
        [SerializeField]
        GameObject m_MarkerPrefab;

        [SerializeField]
        Transform m_XROrigin;

        [SerializeField]
        GameObject m_LocationPrefab;

        protected override void Awake()
        {
            base.Awake();
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        void Update()
        {
            if (Pointer.current == null || m_Pressed == false)
                return;

            var touchPosition = Pointer.current.position.ReadValue();

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one will be the closest hit.
                var hitPose = s_Hits[0].pose;

                GameObject selectedMarkerObject = null;
                foreach (var obj in m_PlacedMarkerObjects)
                {
                    if ((obj.transform.position - hitPose.position).magnitude < 0.1)
                    {
                        selectedMarkerObject = obj;
                        break;
                    }
                }

                if (selectedMarkerObject != null)
                {
                    selectedMarkerObject.transform.position = hitPose.position;
                }
                else
                {
                    if (m_PlacedMarkerObjects.Count < MAX_NUMBER_OF_REFERENCE_POINTS)
                    {
                        m_PlacedMarkerObjects.Add(Instantiate(m_MarkerPrefab, hitPose.position, hitPose.rotation));
                    }
                }
            }

            if (m_PlacedMarkerObjects.Count == MAX_NUMBER_OF_REFERENCE_POINTS)
            {
                Vector3[] pointsInLocationOrigin = { new Vector3(10.615f, 6.802f, 2.355f), new Vector3(8.448f, 6.809f, 4.812f), new Vector3(8.734f, 9.936f, 2.708f) };
                Vector3[] pointsInXROrigin = { m_PlacedMarkerObjects[0].transform.position,
                                               m_PlacedMarkerObjects[1].transform.position,
                                               m_PlacedMarkerObjects[2].transform.position };


                float matchingDeviation = 0.0f;
                Matrix4x4 xrOriginTLocationOrigin = calculateTransfromFromeAToB(pointsInLocationOrigin, pointsInXROrigin, ref matchingDeviation);
                if (m_LocationObject == null)
                {
                    m_LocationObject = Instantiate(m_LocationPrefab, xrOriginTLocationOrigin.GetPosition(), xrOriginTLocationOrigin.rotation, m_XROrigin);
                    Debug.LogFormat("matching deviation: {0}", matchingDeviation);
                }
                else
                {
                    m_LocationObject.transform.position = xrOriginTLocationOrigin.GetPosition();
                    m_LocationObject.transform.rotation = xrOriginTLocationOrigin.rotation;
                }
            }

        }

        static Matrix4x4 calculateTransfromFromeAToB(IList<Vector3> pointsInA, IList<Vector3> pointsInB, ref float matchingDeviation)
        {
            if (pointsInA.Count != pointsInB.Count || pointsInA.Count != 3)
                return Matrix4x4.identity;
            
            Vector3 aPPointsCenter = (pointsInA[0] + pointsInA[1] + pointsInA[2]) / 3.0f;
            Vector3 bPPointsCenter = (pointsInB[0] + pointsInB[1] + pointsInB[2]) / 3.0f;
            Matrix4x4 aTPoints = Matrix4x4.TRS(aPPointsCenter, Quaternion.LookRotation(pointsInA[0] - pointsInA[2], pointsInA[0] - pointsInA[1]), Vector3.one);
            Matrix4x4 bTPoints = Matrix4x4.TRS(bPPointsCenter, Quaternion.LookRotation(pointsInB[0] - pointsInB[2], pointsInB[0] - pointsInB[1]), Vector3.one);
            Matrix4x4 bTA = bTPoints * Matrix4x4.Inverse(aTPoints);

            matchingDeviation = 0;
            for (int i = 0; i < pointsInA.Count; ++i)
            {
                matchingDeviation += (pointsInB[i] - bTA.MultiplyPoint(pointsInA[i])).magnitude;
            }
            matchingDeviation /= pointsInA.Count;

            return bTA;
        }

        protected override void OnPress(Vector3 position) => m_Pressed = true;

        protected override void OnPressCancel() => m_Pressed = false;

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_RaycastManager;
        bool m_Pressed;

        const int MAX_NUMBER_OF_REFERENCE_POINTS = 3;
        List<GameObject> m_PlacedMarkerObjects = new List<GameObject>();
        GameObject m_LocationObject;
    }
}

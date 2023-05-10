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
                //Vector3 locationOriginPp01 = new Vector3(-11.2624f, -2.74274f, -6.8056f);
                //Vector3 locationOriginPp02 = new Vector3(-10.6223f, -2.36561f, -6.79544f);
                //Vector3 locationOriginPp03 = new Vector3(-8.82212f, -2.12815f, -6.81208f);
                //Vector3 locationOriginPp04 = new Vector3(-8.46449f, -4.83936f, -6.81208f);
                //Vector3 locationOriginPp05 = new Vector3(-8.10017f, -7.60133f, -6.81208f);
                //Vector3 locationOriginPp06 = new Vector3(-8.10017f, -7.60133f, -9.70552f);
                //Vector3 locationOriginPp07 = new Vector3(-8.74312f, -2.72705f, -9.94253f);

                Vector3 locationOriginPp01 = new Vector3(10.615f, 6.802f, 2.355f);
                Vector3 locationOriginPp02 = new Vector3(8.448f, 6.809f, 4.812f);
                Vector3 locationOriginPp03 = new Vector3(8.734f, 9.936f, 2.708f);

                Matrix4x4 locationOriginTMarker = Matrix4x4.TRS(locationOriginPp01, Quaternion.LookRotation(locationOriginPp01 - locationOriginPp03, locationOriginPp01 - locationOriginPp02), Vector3.one);


                Vector3 xrOriginPp01 = m_PlacedMarkerObjects[0].transform.position;
                Vector3 xrOriginPp02 = m_PlacedMarkerObjects[1].transform.position;
                Vector3 xrOriginPp03 = m_PlacedMarkerObjects[2].transform.position;

                Matrix4x4 xrOriginTMarker = Matrix4x4.TRS(xrOriginPp01, Quaternion.LookRotation(xrOriginPp01 - xrOriginPp03, xrOriginPp01 - xrOriginPp02), Vector3.one);

                Matrix4x4 xrOriginTLocationOrigin = xrOriginTMarker * Matrix4x4.Inverse(locationOriginTMarker);
                Debug.Log("xr T location: " + xrOriginTLocationOrigin.GetPosition().ToString() + ", " + xrOriginTLocationOrigin.rotation.ToString());
                if (m_LocationObject == null)
                {
                    m_LocationObject = Instantiate(m_LocationPrefab, xrOriginTLocationOrigin.GetPosition(), xrOriginTLocationOrigin.rotation, m_XROrigin);
                }
                else
                {
                    m_LocationObject.transform.position = xrOriginTLocationOrigin.GetPosition();
                    m_LocationObject.transform.rotation = xrOriginTLocationOrigin.rotation;
                }
            }

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

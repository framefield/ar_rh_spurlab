using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public class XRSimulationFixes : MonoBehaviour
    {
#if UNITY_EDITOR

        public void Awake()
        {
            _planeManager = GetComponent<ARPlaneManager>();
        }

        public void OnEnable()
        {
            ARSession.stateChanged += ARSessionOnstateChanged;


            if (_planeManager.enabled)
            {
                // plane mgr gets stuck
                _planeManager.enabled = false;
                Invoke(nameof(ReEnable), 0.1f);
            }
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= ARSessionOnstateChanged;
        }

        private void ARSessionOnstateChanged(ARSessionStateChangedEventArgs args)
        {
            if (args.state == ARSessionState.SessionInitializing && _planeManager.enabled)
            {
                // plane mgr gets stuck
                _planeManager.enabled = false;
                Invoke(nameof(ReEnable), 0.1f);
            }
        }

        private void ReEnable()
        {
            if (enabled)
            {
                _planeManager.enabled = true;
            }
        }

        private ARPlaneManager _planeManager;
#endif
    }
}
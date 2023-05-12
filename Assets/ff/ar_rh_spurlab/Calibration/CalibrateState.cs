using System.Linq;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrateState : MonoBehaviour, IActiveInStateContent
    {
        private CalibrationController _calibrationController;

        public void Initialize()
        {
            _calibrationController = GetComponentInParent<CalibrationController>();
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            var placedMarker = FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();

            for (var i = placedMarker.Count - 1; i >= 0; i--)
                Destroy(placedMarker[i].gameObject);
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}
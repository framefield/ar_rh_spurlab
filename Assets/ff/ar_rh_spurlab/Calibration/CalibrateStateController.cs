using System.Linq;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrateStateController : MonoBehaviour, IActiveInStateContent
    {
        private CalibrationController _calibrationController;

        public void Initialize()
        {
            _calibrationController = GetComponentInParent<CalibrationController>();
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            if (!_calibrationController)
            {
                return;
            }

            if (_calibrationController.Session.didStart)
            {
                _calibrationController.Session.Reset();
            }
            
            _calibrationController.CalibrationData.Reset();
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}
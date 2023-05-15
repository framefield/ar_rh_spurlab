using ff.common.statemachine;
using UnityEngine;

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

            _calibrationController.ResetCalibration();
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}
using ff.common.statemachine;
using UnityEngine;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationLocationSelection : MonoBehaviour, IActiveInStateContent
    {
        private CalibrationController _calibrationController;

        public void Initialize()
        {
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            if (!_calibrationController)
            {
                _calibrationController = stateMachine.GetComponent<CalibrationController>();
            }

            _calibrationController.SetLocation("FF Office");
            stateMachine.Continue();
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}
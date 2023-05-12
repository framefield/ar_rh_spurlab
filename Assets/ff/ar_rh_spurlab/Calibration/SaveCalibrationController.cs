using ff.common.statemachine;
using UnityEngine;

namespace ff.ar_rh_spurlab.Calibration
{
    public class SaveCalibrationController : MonoBehaviour, IActiveInStateContent
    {
        public void Initialize()
        {
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            var calibrationController = stateMachine.GetComponent<CalibrationController>();

            if (calibrationController)
            {
                calibrationController.SaveCalibrationData();
            }
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}
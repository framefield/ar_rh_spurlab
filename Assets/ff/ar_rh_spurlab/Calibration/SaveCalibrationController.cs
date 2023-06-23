using System;
using System.Threading.Tasks;
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
            PerformSave(stateMachine);
        }

        private async void PerformSave(StateMachine stateMachine)
        {
            var calibrationController = stateMachine.GetComponent<CalibrationController>();
            if (calibrationController)
            {
                try
                {
                    await calibrationController.SaveCalibrationData();
                }
                catch (Exception e)
                {
                    Debug.LogError("CalibrationController: Calibration data could not be saved.", this);
                    Debug.LogException(e);
                }

                stateMachine.Continue();
            }
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
        }
    }
}

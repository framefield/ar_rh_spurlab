using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARKit;

namespace ff.ar_rh_spurlab.Calibration
{
    public class ScanEnoughEnvironmentController : MonoBehaviour, IActiveInStateContent
    {
        private CalibrationController _calibrationController;
        private bool _isActive;
        private ARKitSessionSubsystem _sessionSubsystem;

        private StateMachine _stateMachine;

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

#if UNITY_IOS
            if (_sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Mapped)
            {
                _stateMachine.Continue();
            }
#endif
        }

        public void Initialize()
        {
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _stateMachine = stateMachine;

            if (!_calibrationController)
            {
                _calibrationController = stateMachine.GetComponent<CalibrationController>();
            }


            _sessionSubsystem = _calibrationController.Session.subsystem as ARKitSessionSubsystem;
            _isActive = _calibrationController != null && _sessionSubsystem != null;

            if (!_isActive)
            {
                _stateMachine.Continue();
            }
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _isActive = false;
        }
    }
}
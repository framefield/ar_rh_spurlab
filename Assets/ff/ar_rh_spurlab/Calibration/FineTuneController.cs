using System.Collections.Generic;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

namespace ff.ar_rh_spurlab.Calibration
{
    public class FineTuneController : PressInputBase, IActiveInStateContent
    {
        [SerializeField]
        private CalibrationFineTuneUi _calibrationFineTuneUiPrefab;

        private readonly List<ARRaycastHit> _sHits = new();
        private CalibrationController _calibrationController;
        private Camera _mainCamera;
        private bool _isActive;

        private bool _pressed;
        private Vector3 _lastWorldPTouchPosition = Vector3.zero;
        private bool _offsetValid = false;

        private StateMachine _stateMachine;
        private CalibrationFineTuneUi _calibrationFineTuneUi;

        private void Update()
        {
            if (!_isActive || !_calibrationController || !_mainCamera)
            {
                return;
            }

            if (Pointer.current == null || !_pressed)
            {
                _offsetValid = false;
                return;
            }

            var touchPosition = Pointer.current.position.ReadValue();

            var worldPTouchPosition = _mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, _mainCamera.nearClipPlane));
            var worldPOffset = worldPTouchPosition - _lastWorldPTouchPosition;
            if (_offsetValid)
            {
                var calibration = _calibrationController.CalibrationData;
                calibration.Offset += worldPOffset;
            }

            _lastWorldPTouchPosition = worldPTouchPosition;
            _offsetValid = true;
        }


        public void Initialize()
        {
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            if (!_calibrationController)
            {
                _calibrationController = stateMachine.GetComponent<CalibrationController>();
            }

            if (!_calibrationFineTuneUiPrefab)
            {
                Debug.LogError("FineTuneController: CalibrationFineTuneUiPrefab is not set!");
                return;
            }

            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
            }

            _stateMachine = stateMachine;
            _calibrationFineTuneUi = Instantiate(_calibrationFineTuneUiPrefab, transform);
            _calibrationFineTuneUi.OnContinueButtonClicked += () => _stateMachine.Continue();
            _isActive = true;
            _offsetValid = false;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            Destroy(_calibrationFineTuneUi.gameObject);
            _isActive = false;
        }

        protected override void OnPress(Vector3 position)
        {
            _pressed = true;
        }

        protected override void OnPressCancel()
        {
            _pressed = false;
        }
    }
}
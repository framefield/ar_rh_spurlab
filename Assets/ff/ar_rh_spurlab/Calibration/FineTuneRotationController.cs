using System.Collections.Generic;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

namespace ff.ar_rh_spurlab.Calibration
{
    public class FineTuneRotationController : PressInputBase, IActiveInStateContent
    {
        [SerializeField]
        private CalibrationFineTuneRotationUi _calibrationFineTuneRotationUiPrefab;

        private readonly List<ARRaycastHit> _sHits = new();
        private CalibrationController _calibrationController;
        private Camera _mainCamera;
        private bool _isActive;
        private Matrix4x4 _previousOffset;

        private bool _pressed;
        private Vector3 _lastWorldPTouchDirection = Vector3.zero;
        private bool _offsetValid = false;

        private StateMachine _stateMachine;
        private CalibrationFineTuneRotationUi _calibrationFineTuneRotationUi;

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
            var worldPEyePosition = _mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0));
            var worldPTouchDirection = Vector3.Normalize(worldPTouchPosition - worldPEyePosition);

            if (_offsetValid)
            {
                var deltaAngle = Vector3.Angle(_lastWorldPTouchDirection, worldPTouchDirection);
                var moveToEye = Matrix4x4.Translate(-worldPEyePosition);
                var moveToOrigin = moveToEye.inverse;
                var deltaRotation = Matrix4x4.Rotate(Quaternion.AngleAxis(deltaAngle, Vector3.Cross(_lastWorldPTouchDirection, worldPTouchDirection)));
                var calibration = _calibrationController.CalibrationData;
                calibration.Offset = moveToOrigin * deltaRotation * moveToEye * calibration.Offset;
            }

            _lastWorldPTouchDirection = worldPTouchDirection;
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

            if (!_calibrationFineTuneRotationUiPrefab)
            {
                Debug.LogError("FineTuneController: CalibrationFineTuneRoationUiPrefab is not set!");
                return;
            }

            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
            }

            _previousOffset = _calibrationController.CalibrationData.Offset;

            _stateMachine = stateMachine;
            _calibrationFineTuneRotationUi = Instantiate(_calibrationFineTuneRotationUiPrefab, transform);
            _calibrationFineTuneRotationUi.OnContinueButtonClicked += () => _stateMachine.Continue();
            _calibrationFineTuneRotationUi.OnBackButtonClicked += delegate() {
                _calibrationController.CalibrationData.Offset = _previousOffset;
                _stateMachine.Back();
            };
            _isActive = true;
            _offsetValid = false;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            Destroy(_calibrationFineTuneRotationUi.gameObject);
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
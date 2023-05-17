using System.Collections.Generic;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

namespace ff.ar_rh_spurlab.Calibration
{
    public class FineTuneTranslationController : PressInputBase, IActiveInStateContent
    {
        [SerializeField]
        private CalibrationFineTuneTranslationUi _calibrationFineTuneTranslationUiPrefab;

        private readonly List<ARRaycastHit> _sHits = new();
        private CalibrationController _calibrationController;
        private Camera _mainCamera;
        private bool _isActive;
        private Matrix4x4 _previousOffset;

        private bool _pressed;
        private Vector3 _lastWorldPTouchPosition = Vector3.zero;
        private bool _offsetValid = false;

        private StateMachine _stateMachine;
        private CalibrationFineTuneTranslationUi _calibrationFineTuneTranslationUi;

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

            if (_offsetValid)
            {
                var calibration = _calibrationController.CalibrationData;
                var worldPOffset = worldPTouchPosition - _lastWorldPTouchPosition;
                calibration.Offset = Matrix4x4.Translate(worldPOffset) * calibration.Offset;
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

            if (!_calibrationFineTuneTranslationUiPrefab)
            {
                Debug.LogError("FineTuneController: CalibrationFineTuneUiPrefab is not set!");
                return;
            }

            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
            }

            _previousOffset = _calibrationController.CalibrationData.Offset;

            _stateMachine = stateMachine;
            _calibrationFineTuneTranslationUi = Instantiate(_calibrationFineTuneTranslationUiPrefab, transform);
            _calibrationFineTuneTranslationUi.OnContinueButtonClicked += () => _stateMachine.Continue();
            _calibrationFineTuneTranslationUi.OnBackButtonClicked += delegate() {
                _calibrationController.CalibrationData.Offset = _previousOffset;
                _stateMachine.Back();
            };
            _isActive = true;
            _offsetValid = false;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            Destroy(_calibrationFineTuneTranslationUi.gameObject);
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
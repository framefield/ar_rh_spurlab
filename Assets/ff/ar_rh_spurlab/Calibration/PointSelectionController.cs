using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab.Locations;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.Calibration
{
    public class PointSelectionController : PressInputBase, IActiveInStateContent
    {
        [SerializeField]
        private ARAnchor _markerPrefab;

        [SerializeField]
        private PointSelectionUi _pointSelectionUiPrefab;

        private readonly List<ARAnchor> _placedAnchors = new();

        private readonly List<ARRaycastHit> _sHits = new();

        private CalibrationController _calibrationController;
        private bool _isActive;
        private PointSelectionUi _pointSelectionUi;

        private bool _pressed;

        private StateMachine _stateMachine;

        public bool IsReady => _placedAnchors.Count == LocationData.NumberOfReferencePoints;

        private void Update()
        {
            if (!_isActive || !_calibrationController)
            {
                return;
            }

            if (Pointer.current == null || !_pressed)
            {
                return;
            }

            var touchPosition = Pointer.current.position.ReadValue();

            if (!_calibrationController.RaycastManager.Raycast(touchPosition, _sHits, TrackableType.PlaneWithinPolygon))
            {
                return;
            }

            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            var hitPose = _sHits[0].pose;

            var draggedAnchor = (from placedAnchor in _placedAnchors
                where Vector3.Distance(placedAnchor.transform.position, hitPose.position) < 0.1f
                select placedAnchor.transform).FirstOrDefault();

            if (draggedAnchor)
            {
                draggedAnchor.position = hitPose.position;
                return;
            }

            if (IsReady)
            {
                return;
            }

            var marker = Instantiate(_markerPrefab, hitPose.position, hitPose.rotation,
                _calibrationController.XrOrigin);

            marker.name = "ARMarkerAnchor";
            _placedAnchors.Add(marker);
            _calibrationController.CalibrationData.MatchedAnchors.Add(marker);
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

            if (_calibrationController.CalibrationData == null)
            {
                return;
            }

            _stateMachine = stateMachine;
            _isActive = true;

            _pointSelectionUi = Instantiate(_pointSelectionUiPrefab, transform);
            _pointSelectionUi.SetPointSelectionController(this);
            _pointSelectionUi.OnContinueButtonClicked += () => _stateMachine.Continue();

            foreach (var placedAnchor in _placedAnchors)
            {
                if (placedAnchor || placedAnchor.gameObject)
                {
                    Destroy(placedAnchor.gameObject);
                }
            }

            _placedAnchors.Clear();

            _calibrationController.CalibrationData.MatchedAnchors.Clear();
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _isActive = false;

            if (_pointSelectionUi)
            {
                Destroy(_pointSelectionUi.gameObject);
            }
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
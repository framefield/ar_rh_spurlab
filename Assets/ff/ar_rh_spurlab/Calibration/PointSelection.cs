using System;
using System.Collections.Generic;
using System.Linq;
using ff.common.statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.Calibration
{
    public class PointSelection : PressInputBase, IActiveInStateContent
    {
        [SerializeField]
        private GameObject _markerPrefab;

        private readonly List<ARRaycastHit> _sHits = new();
        private CalibrationAnchorController _calibrationAnchorController;
        private CalibrationController _calibrationController;
        private bool _isActive;

        private bool _pressed;

        private StateMachine _stateMachine;

        private void Update()
        {
            if (!_isActive || !_calibrationController)
            {
                return;
            }

            var calibration = _calibrationController.CalibrationData;
            //_calibrationAnchorController.Update();

            //var placedMarker = FindObjectsByType<ARAnchor>(FindObjectsSortMode.None).ToList();
            //placedMarker.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name, StringComparison.Ordinal));

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

            GameObject selectedMarkerObject = null;
            foreach (var obj in calibration.Markers)
                if ((obj.Position - hitPose.position).magnitude < 0.1)
                {
                    selectedMarkerObject = obj.GameObject;
                    break;
                }

            if (selectedMarkerObject != null)
            {
                selectedMarkerObject.transform.position = hitPose.position;
            }
            else
            {
                if (calibration.Markers.Count < calibration.NumberOfReferencePoints)
                {
                    var marker = Instantiate(_markerPrefab, hitPose.position, hitPose.rotation,
                        _calibrationController.XrOrigin);
                    //var arAnchor = marker.GetComponent<ARAnchor>();
                    var calibrationId = $"ARMarkerAnchor_{calibration.Markers.Count}";
                    marker.name = calibrationId;
                    //placedMarker.Add(arAnchor);
                    calibration.Markers.Add(new Marker(marker));

                    //_calibrationAnchorController.TryAddCalibrationId(calibrationId, arAnchor);
                }
            }


            if (calibration.Markers.Count == calibration.NumberOfReferencePoints)
            {
                //calibration.UpdateMarkers(placedMarker[0], placedMarker[1], placedMarker[2]);
                _stateMachine.Continue();
            }
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

            if (_calibrationController)
            {
                _calibrationAnchorController = new CalibrationAnchorController(_calibrationController.AnchorManager);
            }

            _stateMachine = stateMachine;
            _isActive = true;
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
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
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.Calibration
{
    public class CalibrationAnchorController
    {
        private readonly Dictionary<TrackableId, string> _calibrationIdByTrackableId = new();

        private readonly List<(string calibrationId, ARAnchor arAnchor)> _pendingAnchors = new();

        public CalibrationAnchorController(ARAnchorManager arAnchorManager)
        {
            arAnchorManager.anchorsChanged += AnchorsChangedHandler;
        }

        public void TryAddCalibrationId(string calibrationId, ARAnchor arAnchor)
        {
            if (arAnchor.pending)
            {
                _pendingAnchors.Add((calibrationId, arAnchor));
            }
        }

        public void Update()
        {
            for (var i = _pendingAnchors.Count - 1; i >= 0; i--)
            {
                var pendingAnchor = _pendingAnchors[i];
                if (!pendingAnchor.arAnchor.pending)
                {
                    AddCalibrationId(pendingAnchor.calibrationId, pendingAnchor.arAnchor.trackableId);
                }

                _pendingAnchors.RemoveAt(i);
            }
        }

        private void AddCalibrationId(string calibrationId, TrackableId trackableId)
        {
            _calibrationIdByTrackableId[trackableId] = calibrationId;
        }

        private void AnchorsChangedHandler(ARAnchorsChangedEventArgs changedEventArgs)
        {
            foreach (var addedAnchor in changedEventArgs.added)
                if (_calibrationIdByTrackableId.TryGetValue(addedAnchor.trackableId, out var value))
                {
                    addedAnchor.name = value;
                }
        }
    }
}
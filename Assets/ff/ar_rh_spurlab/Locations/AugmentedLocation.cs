using System;
using ff.ar_rh_spurlab.Calibration;
using ff.ar_rh_spurlab.GrayScaler;
using ff.ar_rh_spurlab.UI.Site_Ui;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public enum WorldMapState
    {
        None,
        Loading,
        Loaded,
        Failed,
        NotFound
    }

    public enum LocationTrackingState
    {
        None,
        AwaitingWorldMap,
        WaitingForAnchors,
        TrackingAnchors,
        TrackingCalibration,
    }

    public struct LocationTrackingData
    {
        public LocationTrackingState State;
        public float Quality;
        public string CalibrationMessage;
    }

    public class AugmentedLocation : MonoBehaviour
    {
        public LocationData LocationData { get; private set; }
        public CalibrationData CalibrationData { get; private set; }

        public LocationTrackingData TrackingData { get; private set; }

        public event Action<LocationTrackingState> OnTrackingStateChanged;
        public event Action<LocationTrackingData> OnTrackingDataChanged;


        public void SetWorldMapState(WorldMapState worldMapState)
        {
            _worldMapState = worldMapState;
        }

        private void Update()
        {
            var areAnchorsReady = CalibrationData?.AreAnchorsReady == true;
            var oldTrackingData = TrackingData;

            if (!areAnchorsReady || _worldMapState != WorldMapState.Loaded)
            {
                // update tracking data
                TrackingData = new LocationTrackingData
                {
                    State =
                        _worldMapState == WorldMapState.Loaded
                            ? LocationTrackingState.WaitingForAnchors
                            : LocationTrackingState.AwaitingWorldMap,
                    Quality = CalibrationData != null
                        ? CalibrationData.MatchedAnchorsCount * (0.25f / LocationData.NumberOfReferencePoints)
                        : 0,
                    CalibrationMessage = _worldMapState != WorldMapState.Loaded
                        ? $"ArWorldMap {_worldMapState}"
                        : CalibrationCalculator.GetCalibrationMessage(CalibrationData, LocationData)
                };

                if (oldTrackingData.State > LocationTrackingState.WaitingForAnchors)
                {
                    // update tracked location contents
                    foreach (var trackedLocationContent in _trackedLocationContents)
                    {
                        trackedLocationContent.SetIsTracked(false);
                    }

                    OnTrackingStateChanged?.Invoke(TrackingData.State);
                }

                OnTrackingDataChanged?.Invoke(TrackingData);
                return;
            }

            // update transform
            var (xrOriginTLocationOrigin, isValid, quality) =
                CalibrationCalculator.GetXrOriginTLocationOrigin(CalibrationData, LocationData);

            var localTransform = transform;
            localTransform.position = xrOriginTLocationOrigin.GetPosition();
            localTransform.rotation = xrOriginTLocationOrigin.rotation;

            // update tracking data
            TrackingData = new LocationTrackingData()
            {
                State = isValid ? LocationTrackingState.TrackingCalibration : LocationTrackingState.TrackingAnchors,
                Quality = CalibrationData.MatchedAnchorsCount * (0.25f / LocationData.NumberOfReferencePoints) +
                          quality * 0.75f,
                CalibrationMessage = isValid ? string.Empty : "Anchors do not match calibration data enough"
            };

            // update tracked location contents
            var wasValid = oldTrackingData.State == LocationTrackingState.TrackingCalibration;
            if (wasValid != isValid)
            {
                foreach (var trackedLocationContent in _trackedLocationContents)
                {
                    trackedLocationContent.SetIsTracked(isValid);
                }
            }

            if (oldTrackingData.State != TrackingData.State)
            {
                OnTrackingStateChanged?.Invoke(TrackingData.State);
            }

            OnTrackingDataChanged?.Invoke(TrackingData);
        }

        public void Initialize(CalibrationData calibrationData, LocationData locationData)
        {
            CalibrationData = calibrationData;
            LocationData = locationData;

            _trackedLocationContents = GetComponentsInChildren<ITrackedLocationContent>();
            Debug.Log($"Location.Initialize: {name} has {_trackedLocationContents.Length} tracked location contents");

            foreach (var trackedLocationContent in _trackedLocationContents)
            {
                trackedLocationContent.Initialize();
            }

            var portal = GetComponentInChildren<Portal>();
            if (portal)
            {
                portal.OnEnter += OnPortalEnterHandler;
            }
        }

        private void OnPortalEnterHandler()
        {
            SharedLocationContext.VisitedLocationIds.Add(LocationData.Id);
        }

        private ITrackedLocationContent[] _trackedLocationContents;
        private WorldMapState _worldMapState;
    }
}

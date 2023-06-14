using System;
using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    public enum PositioningStatus
    {
        Stopped,
        Initializing,
        Running,
        Failed
    }

    public struct PositioningInfo
    {
        public double Timestamp;
        public GeoPosition GeoPosition;
        public double Heading;
        public float HeadingAccuracy;
        public float HorizontalAccuracy;
        public float VerticalAccuracy;
    }

    public class PositioningService : MonoBehaviour
    {
        public event Action<PositioningStatus> OnStatusChanged;
        public event Action<PositioningInfo> OnPositionChanged;

        public PositioningStatus Status { get; private set; } = PositioningStatus.Stopped;
        public PositioningInfo LastPosition { get; private set; }

        private void OnEnable()
        {
#if UNITY_IOS
            // Check if the user has location service enabled.
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("User has not enabled GPS");
                enabled = false;
                return;
            }
#endif
            Input.compass.enabled = true;
            Input.location.Start(5, 5);
        }

        private void OnDisable()
        {
            Input.location.Stop();
        }

        private void Update()
        {
            var newStatus = ConvertStatus(Input.location.status);

            if (Status != newStatus)
            {
                Status = newStatus;
                OnStatusChanged?.Invoke(Status);
            }

            if (Input.location.status == LocationServiceStatus.Running)
            {
                var locationInfo = Input.location.lastData;
                var lastTimestamp = LastPosition.Timestamp;
                if (locationInfo.timestamp > lastTimestamp || Input.compass.timestamp > lastTimestamp)
                {
                    LastPosition = new PositioningInfo
                    {
                        Timestamp = Math.Max(locationInfo.timestamp, Input.compass.timestamp),
                        Heading = Input.compass.trueHeading,
                        HeadingAccuracy = Input.compass.headingAccuracy,
                        GeoPosition = new GeoPosition
                        {
                            Latitude = locationInfo.latitude,
                            Longitude = locationInfo.longitude,
                            Altitude = locationInfo.altitude
                        },
                        HorizontalAccuracy = locationInfo.horizontalAccuracy,
                        VerticalAccuracy = locationInfo.verticalAccuracy
                    };

                    OnPositionChanged?.Invoke(LastPosition);
                }
            }
        }

        private static PositioningStatus ConvertStatus(LocationServiceStatus locationStatus)
        {
            return locationStatus switch
            {
                LocationServiceStatus.Stopped => PositioningStatus.Stopped,
                LocationServiceStatus.Initializing => PositioningStatus.Initializing,
                LocationServiceStatus.Running => PositioningStatus.Running,
                LocationServiceStatus.Failed => PositioningStatus.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(locationStatus), locationStatus, null)
            };
        }

        public void OverridePosition(GeoPosition positionOverride, double headingOverride, float verticalAccuracy,
            float horizontalAccuracy, float headingAccuracy)
        {
            Status = PositioningStatus.Running;
            LastPosition = new PositioningInfo
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
                Heading = headingOverride,
                HeadingAccuracy = headingAccuracy,
                GeoPosition = positionOverride,
                HorizontalAccuracy = horizontalAccuracy,
                VerticalAccuracy = verticalAccuracy
            };
            OnStatusChanged?.Invoke(Status);
            OnPositionChanged?.Invoke(LastPosition);
        }

        private static PositioningService _cachedService;

        public static PositioningService FindFirst()
        {
            if (_cachedService)
            {
                return _cachedService;
            }

            _cachedService = FindFirstObjectByType<PositioningService>();
            return _cachedService;
        }
    }
}

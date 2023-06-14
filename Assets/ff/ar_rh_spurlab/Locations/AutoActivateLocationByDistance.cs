using System.Linq;
using ff.ar_rh_spurlab.Positioning;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class AutoActivateLocationByDistance : MonoBehaviour
    {
        [SerializeField]
        private float _activationDistanceInMeters = 50f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float _accuracyFactor = -0.5f;

        [SerializeField]
        private float _activationDistanceInMetersAfterUserLocationSelection = 10f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float _accuracyFactorAfterUserLocationSelection = 0.5f;

        [SerializeField]
        private float _maxAllowedAccuracyInMeters = 50f;

        private LocationController _locationController;
        private PositioningService _positioningService;
        private ChangeSource _lastChangeSource;
        private LocationData[] _locations;

        private void Awake()
        {
            _locationController = LocationController.FindFirst();
            _positioningService = PositioningService.FindFirst();
            var availableSites = AvailableSites.LoadFromResources();

            if (availableSites != null)
            {
                _locations = availableSites.Sites.SelectMany(s => s.Locations).ToArray();
            }
            else
            {
                enabled = false;
            }

            if (!_locationController)
            {
                Debug.LogError("LocationController not found", this);
                enabled = false;
            }

            if (!_positioningService)
            {
                Debug.LogError("PositioningService not found", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_locationController)
            {
                _locationController.LocationChanged += OnLocationChangedHandler;
                OnLocationChangedHandler(ChangeSource.Unknown);
            }

            if (_positioningService)
            {
                _positioningService.OnPositionChanged += OnPositionChangedHandler;
            }
        }

        private void OnDisable()
        {
            if (_locationController)
            {
                _locationController.LocationChanged -= OnLocationChangedHandler;
            }

            if (_positioningService)
            {
                _positioningService.OnPositionChanged -= OnPositionChangedHandler;
            }
        }

        private void OnLocationChangedHandler(ChangeSource change)
        {
            _lastChangeSource = change;
        }

        private void OnPositionChangedHandler(PositioningInfo info)
        {
            // ignore if accuracy is too low
            if (info.HorizontalAccuracy > _maxAllowedAccuracyInMeters)
            {
                return;
            }

            var maxDistance = _lastChangeSource == ChangeSource.User
                ? _activationDistanceInMetersAfterUserLocationSelection
                : _activationDistanceInMeters;

            var foundInDistance = 0;
            var closestDistance = double.MaxValue;
            var closestLocation = default(LocationData);
            var activeLocationFoundAgain = false;
            foreach (var location in _locations)
            {
                var distance = info.GeoPosition.HaversineDistance(location.GeoPosition);
                var closeEnough = _lastChangeSource == ChangeSource.User
                    // 
                    ? (distance + _accuracyFactorAfterUserLocationSelection * info.HorizontalAccuracy) < maxDistance
                    : (distance + _accuracyFactor * info.HorizontalAccuracy) < maxDistance;
                if (closeEnough)
                {
                    foundInDistance++;

                    if (_locationController.CurrentLocation &&
                        closestLocation == _locationController.CurrentLocation.LocationData)
                    {
                        activeLocationFoundAgain = true;
                    }

                    if (distance < closestDistance)
                    {
                        closestLocation = location;
                        closestDistance = distance;
                    }
                }

                //Debug.Log($"Distance to {location.Id} {distance:F1}m closeEnough:{closeEnough} {_lastChangeSource}", this);
            }

            var shouldActivate =
                // no location was activated yet
                _lastChangeSource == ChangeSource.Unknown || _lastChangeSource == ChangeSource.Start ||
                !_locationController.CurrentLocation ||
                // prevent jumping back and forth between locations
                (_lastChangeSource == ChangeSource.User || !activeLocationFoundAgain
                    ?
                    // take any location
                    foundInDistance > 1
                    :
                    // take only exact matched location
                    foundInDistance == 1
                );
            if (shouldActivate && closestLocation)
            {
                if (!_locationController.CurrentLocation ||
                    closestLocation != _locationController.CurrentLocation.LocationData)
                {
                    Debug.Log(
                        $"🗺️ AutoActivateLocationByDistance Enable by distance {closestLocation.Id} {closestDistance:F1}m",
                        this);
                    _locationController.SetLocation(closestLocation, ChangeSource.Gps);
                }
            }
        }
    }
}

using System;
using UnityEngine;

namespace ff.ar_rh_spurlab.GrayScaler
{
    /// <summary>
    ///     Testing implementation of <see cref="IGrayScalePointOfInterest" /> that uses the position of the game object
    /// </summary>
    public class GrayScaleScenePointOfInterest : MonoBehaviour, IGrayScalePointOfInterest
    {
        private CameraBackgroundRenderer _backgroundRenderer;

        private void Awake()
        {
            _backgroundRenderer = FindFirstObjectByType<CameraBackgroundRenderer>();
        }

        private void OnEnable()
        {
            if (_backgroundRenderer)
            {
                _backgroundRenderer.RegisterPointOfInterest(this);
            }
            else
            {
                Debug.LogWarning("Could not find CameraBackgroundRenderer in scene.", this);
            }
        }

        private void OnDisable()
        {
            if (_backgroundRenderer)
            {
                _backgroundRenderer.UnregisterPointOfInterest(this);
            }
        }

        public Vector3 WorldPosition => transform.position;
    }
}
using UnityEngine;

namespace ff.ar_rh_spurlab.GrayScaler
{
    /// <summary>
    ///     Testing implementation of <see cref="IGrayScalePointOfInterest" /> that uses the position of the game object
    /// </summary>
    public class GrayScaleScenePointOfInterest : MonoBehaviour, IGrayScalePointOfInterest
    {
        private CameraBackgroundRenderParams _renderParams;

        private void Awake()
        {
            _renderParams = FindFirstObjectByType<CameraBackgroundRenderParams>();
            _renderParams.RegisterPointOfInterest(this);
        }

        private void OnDestroy()
        {
            _renderParams.UnregisterPointOfInterest(this);
        }

        public Vector3 WorldPosition => transform.position;
    }
}

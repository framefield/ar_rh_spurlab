using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.GrayScaler
{
    public class CameraBackgroundRenderParams : MonoBehaviour
    {
        [SerializeField]
        private AROcclusionManager _occlusionManager;

        [SerializeField]
        private ARCameraBackground _arCameraBackground;

        private Material _material;

        private void Start()
        {
            // Don't do anything if segmentation isn't supported on this device
            if (!SupportsSegmentation())
            {
                enabled = false;
            }

            _material = _arCameraBackground.material;
        }

        private void Update()
        {
        }

        private bool SupportsSegmentation()
        {
            var subsystem = _occlusionManager.subsystem;
            return subsystem != null &&
                   _occlusionManager.currentHumanDepthMode != HumanSegmentationDepthMode.Disabled &&
                   _occlusionManager.currentHumanStencilMode != HumanSegmentationStencilMode.Disabled;
        }
    }
}

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.GrayScaler
{
    public class PeopleOcclusionBackground : MonoBehaviour
    {
        [SerializeField]
        private AROcclusionManager _occlusionManager = default;

        [SerializeField]
        private ARCameraBackground _arCameraBackground = default;

        [SerializeField]
        private Shader _customBackgroundShader = default;

        [SerializeField]
        private float _depthFactor = 1.5f;

        [SerializeField]
        private float _stencilFactor = 1.25f;

        private void Start()
        {
            // Don't do anything if segmentation isn't supported on this device
            if (!SupportsSegmentation())
            {
                enabled = false;
            }

            _material = _arCameraBackground.material;
            _material.shader = _customBackgroundShader;
        }

        private void Update()
        {
            _material.SetTexture(DepthTexId, _occlusionManager.humanDepthTexture);
            _material.SetTexture(StencilTexId, _occlusionManager.humanStencilTexture);
            _material.SetFloat(DepthFactorId, _depthFactor);
            _material.SetFloat(StencilFactorId, _stencilFactor);
        }

        private bool SupportsSegmentation()
        {
            var subsystem = _occlusionManager.subsystem;
            return subsystem != null &&
                   _occlusionManager.currentHumanDepthMode != HumanSegmentationDepthMode.Disabled &&
                   _occlusionManager.currentHumanStencilMode != HumanSegmentationStencilMode.Disabled;
        }

        private const string DepthTexName = "_textureDepth";
        private const string StencilTexName = "_textureStencil";
        private const string DepthFactorName = "_depthFactor";
        private const string StencilFactorName = "_stencilFactor";

        private static readonly int DepthTexId = Shader.PropertyToID(DepthTexName);
        private static readonly int StencilTexId = Shader.PropertyToID(StencilTexName);
        private static readonly int DepthFactorId = Shader.PropertyToID(DepthFactorName);
        private static readonly int StencilFactorId = Shader.PropertyToID(StencilFactorName);

        private Material _material;
        private bool _hasSegmentation;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.GrayScaler
{
    [RequireComponent(typeof(PortalTextureRenderer))]
    public class CameraBackgroundRenderer : MonoBehaviour
    {
        [SerializeField]
        private ARCameraBackground _arCameraBackground;

        [SerializeField]
        private Renderer _desktopBackgroundRenderer;

        private readonly HashSet<IGrayScalePointOfInterest> _pointOfInterests = new();

        private PortalTextureRenderer _portalTextureRenderer;

        private bool _isInsidePortal;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            var arCamera = _arCameraBackground.GetComponent<Camera>();
            _portalTextureRenderer = GetComponent<PortalTextureRenderer>();
            if (arCamera && _portalTextureRenderer)
            {
                _portalTextureRenderer.Initialize(arCamera);
            }
            else
            {
                Debug.LogWarning("Could not initialize portal texture renderer.", this);
                enabled = false;
            }

            var shader = _arCameraBackground.material.shader;
            _modeNoPortalKeyword = new LocalKeyword(shader, "_MODE_NOPORTAL");
            _modeInPortalKeyword = new LocalKeyword(shader, "_MODE_INPORTAL");
            _modeGuideToPortalKeyword = new LocalKeyword(shader, "_MODE_GUIDETOPORTAL");
            _xrSimulationKeyword = new LocalKeyword(shader, "XR_SIMULATION");
        }

        private void OnEnable()
        {
            Portal.OnPortalTriggered += OnPortalTriggeredHandler;
            _isInsidePortal = Portal.ActivePortal != null;
        }

        private void OnDisable()
        {
            Portal.OnPortalTriggered -= OnPortalTriggeredHandler;
            _isInsidePortal = false;
        }

        private void OnPortalTriggeredHandler(Portal portal, bool shouldActivate)
        {
            _isInsidePortal = shouldActivate;
        }

        private void Update()
        {
            var material = _arCameraBackground.material;

#if UNITY_EDITOR
            _desktopBackgroundRenderer.sharedMaterial = material;
#endif

            material.SetMatrix(PointsOfInterestPropId, GetPointsOfInterestMatrix());
            material.SetMatrix(CameraTransformId, _arCameraBackground.transform.localToWorldMatrix);

            // does not have to be updated each frame, we do it anyway for simplicity
            material.SetTexture(PortalMaskPropId, _portalTextureRenderer.PortalTexture);

#if UNITY_EDITOR
            material.EnableKeyword(_xrSimulationKeyword);
#else
            material.DisableKeyword(_xrSimulationKeyword);
#endif

            if (_pointOfInterests.Count > 0)
            {
                if (_isInsidePortal)
                {
                    material.DisableKeyword(_modeNoPortalKeyword);
                    material.EnableKeyword(_modeInPortalKeyword);
                    material.DisableKeyword(_modeGuideToPortalKeyword);
                }
                else
                {
                    material.DisableKeyword(_modeNoPortalKeyword);
                    material.DisableKeyword(_modeInPortalKeyword);
                    material.EnableKeyword(_modeGuideToPortalKeyword);
                }
            }
            else
            {
                material.EnableKeyword(_modeNoPortalKeyword);
                material.DisableKeyword(_modeInPortalKeyword);
                material.DisableKeyword(_modeGuideToPortalKeyword);
            }
        }

        private Matrix4x4 GetPointsOfInterestMatrix()
        {
            // sort the points of interest by distance to the camera
            var cameraPos = _arCameraBackground.transform.position;

            var sortedPointsOfInterest = _pointOfInterests
                .Select(a => a.WorldPosition)
                .OrderBy(a => Vector3.Distance(a, cameraPos))
                .Take(4)
                .ToArray();

            var result = Matrix4x4.zero;
            for (var i = 0; i < sortedPointsOfInterest.Length; i++)
            {
                result.SetRow(i, sortedPointsOfInterest[i]);
                result[i, 3] = 1;
            }

            return result;
        }

        public void RegisterPointOfInterest(GrayScaleScenePointOfInterest pointOfInterest)
        {
            _pointOfInterests.Add(pointOfInterest);
        }

        public void UnregisterPointOfInterest(GrayScaleScenePointOfInterest pointOfInterest)
        {
            _pointOfInterests.Remove(pointOfInterest);
        }

        private void OnDestroy()
        {
            // reset the material to its original state for clean repo
            var material = _arCameraBackground.material;

            material.EnableKeyword(_modeNoPortalKeyword);
            material.DisableKeyword(_modeInPortalKeyword);
            material.DisableKeyword(_modeGuideToPortalKeyword);
        }

        private static readonly int PointsOfInterestPropId = Shader.PropertyToID("_PointsOfInterest");
        private static readonly int CameraTransformId = Shader.PropertyToID("_cameraTransformMatrix");
        private static readonly int PortalMaskPropId = Shader.PropertyToID("_portalMask");

        private LocalKeyword _modeNoPortalKeyword;
        private LocalKeyword _modeInPortalKeyword;
        private LocalKeyword _modeGuideToPortalKeyword;
        private LocalKeyword _xrSimulationKeyword;
    }
}
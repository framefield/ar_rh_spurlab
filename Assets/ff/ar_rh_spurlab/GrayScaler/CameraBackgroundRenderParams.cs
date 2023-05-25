﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.GrayScaler
{
    [RequireComponent(typeof(PortalTextureRenderer))]
    public class CameraBackgroundRenderParams : MonoBehaviour
    {
        private static readonly int PointsOfInterestPropId = Shader.PropertyToID("_PointsOfInterest");
        private static readonly int CameraForwardPropId = Shader.PropertyToID("_cameraForward");
        private static readonly int CameraRightPropId = Shader.PropertyToID("_cameraRight");
        private static readonly int CameraUpPropId = Shader.PropertyToID("_cameraLeft");
        private static readonly int PortalMaskPropId = Shader.PropertyToID("_portalMask");

        [SerializeField]
        private ARCameraBackground _arCameraBackground;

        [SerializeField]
        private Renderer _desktopBackgroundRenderer;

        private readonly HashSet<IGrayScalePointOfInterest> _pointOfInterests = new();

        private PortalTextureRenderer _portalTextureRenderer;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            var material = _arCameraBackground.material;

#if UNITY_EDITOR
            _desktopBackgroundRenderer.sharedMaterial = material;
#endif

            material.SetMatrix(PointsOfInterestPropId, GetPointsOfInterestMatrix());
            material.SetVector(CameraForwardPropId, _arCameraBackground.transform.forward);
            material.SetVector(CameraRightPropId, _arCameraBackground.transform.right);
            material.SetVector(CameraUpPropId, _arCameraBackground.transform.up);
            material.SetTexture(PortalMaskPropId, _portalTextureRenderer.PortalTexture);
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
    }
}

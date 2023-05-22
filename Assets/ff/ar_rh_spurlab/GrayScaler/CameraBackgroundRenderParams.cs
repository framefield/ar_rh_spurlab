using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ff.ar_rh_spurlab.GrayScaler
{
    public class CameraBackgroundRenderParams : MonoBehaviour
    {
        private static readonly int PointsOfInterestPropId = Shader.PropertyToID("_PointsOfInterest");
        private static readonly int CameraForwardPropId = Shader.PropertyToID("_CameraForward");
        private static readonly int CameraRightPropId = Shader.PropertyToID("_CameraRight");
        private static readonly int CameraUpPropId = Shader.PropertyToID("_CameraLeft");

        [SerializeField]
        private AROcclusionManager _occlusionManager;

        [SerializeField]
        private ARCameraBackground _arCameraBackground;

        [SerializeField]
        private Renderer _desktopBackgroundRenderer;

        private readonly HashSet<IGrayScalePointOfInterest> _pointOfInterests = new();

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


        private bool SupportsSegmentation()
        {
            return _occlusionManager != null &&
                   _occlusionManager.currentHumanDepthMode != HumanSegmentationDepthMode.Disabled &&
                   _occlusionManager.currentHumanStencilMode != HumanSegmentationStencilMode.Disabled;
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

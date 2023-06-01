using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ff.ar_rh_spurlab.GrayScaler
{
    internal class PortalTextureRenderer : MonoBehaviour
    {
        public Texture PortalTexture => _portalRenderTexture;

        [SerializeField]
        private int _portalLayer = 16;

        [SerializeField]
        private int _occluderLayer = 17;


        private bool _isInitialized;

        private Camera _portalCamera;
        private SphereCollider _portalCollider;

        private RenderTexture _portalRenderTexture;

        private Camera _xrCamera;

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            CopyLimitedSettingsToCamera(_xrCamera, _portalCamera, _portalCollider);

            _portalCamera.Render();

            if (!_portalRenderTexture.IsCreated() && !_portalRenderTexture.Create())
            {
                return;
            }
        }

        private void OnDestroy()
        {
            if (_portalCamera != null)
            {
                _portalCamera.targetTexture = null;
            }

            if (_portalRenderTexture != null)
            {
                _portalRenderTexture.Release();
            }
        }

        public void Initialize(Camera xrCamera)
        {
            if (_isInitialized)
            {
                return;
            }

            var go = GameObjectUtils.Create("Portal Camera");
            //go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = _portalLayer;

            var rigidBody = go.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            _portalCollider = go.AddComponent<SphereCollider>();
            _portalCollider.radius = 0.025f;

            _portalCamera = go.AddComponent<Camera>();
            _portalCamera.enabled = false;

            _portalCamera.depth = xrCamera.depth - 1;
            _portalCamera.cullingMask = 1 << _portalLayer | 1 << _occluderLayer;
            _portalCamera.clearFlags = CameraClearFlags.Color;
            _portalCamera.backgroundColor = Color.clear;

            _xrCamera = xrCamera;

            CopyLimitedSettingsToCamera(_xrCamera, _portalCamera, _portalCollider);

            var descriptor = new RenderTextureDescriptor(_xrCamera.scaledPixelWidth, _xrCamera.scaledPixelHeight);

            // Need to make sure we set the graphics format to our valid format
            // or we will get an out of range value for the render texture format
            // when we try creating the render texture
            descriptor.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
            // Need to enable depth buffer if the target camera did not already have it.
            if (descriptor.depthBufferBits < 24)
            {
                descriptor.depthBufferBits = 24;
            }

            _portalRenderTexture = new RenderTexture(descriptor)
            {
                name = "Portal Render Texture",
                hideFlags = HideFlags.HideAndDontSave
            };

            if (_portalRenderTexture.Create())
            {
                _portalCamera.targetTexture = _portalRenderTexture;
            }

            _isInitialized = true;
        }


        private static void CopyLimitedSettingsToCamera(Camera source, Camera destination, SphereCollider collider)
        {
            var layer = destination.gameObject.layer;
            var scene = destination.scene;
            var cullingMask = destination.cullingMask;
            var clearFlags = destination.clearFlags;
            var backgroundColor = destination.backgroundColor;
            var depth = destination.depth;
            var targetTexture = destination.targetTexture;

            destination.CopyFrom(source);
            destination.projectionMatrix = source.projectionMatrix;

            collider.center = new Vector3(0, 0, source.nearClipPlane);

            destination.gameObject.layer = layer;
            destination.scene = scene;
            destination.cullingMask = cullingMask;
            destination.clearFlags = clearFlags;
            destination.backgroundColor = backgroundColor;
            destination.depth = depth;
            destination.targetTexture = targetTexture;
        }
    }
}
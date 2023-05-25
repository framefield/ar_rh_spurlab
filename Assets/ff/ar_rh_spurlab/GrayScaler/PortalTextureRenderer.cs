using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ff.ar_rh_spurlab.GrayScaler
{
    internal class PortalTextureRenderer : MonoBehaviour
    {
        public Texture2D PortalTexture => _portalTexture;

        [SerializeField]
        private int _portalLayer = 16;

        [SerializeField]
        private int _occluderLayer = 17;


        private bool _isInitialized;
        private Camera _portalCamera;

        private RenderTexture _portalRenderTexture;
        private Texture2D _portalTexture;

        private Camera _xrCamera;

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // Currently assuming the main camera is being set to the correct settings for rendering to the target device
            _xrCamera.ResetProjectionMatrix();
            CopyLimitedSettingsToCamera(_xrCamera, _portalCamera);

            _portalCamera.Render();

            if (!_portalRenderTexture.IsCreated() && !_portalRenderTexture.Create())
            {
                return;
            }

            if (_portalTexture.width != _portalRenderTexture.width
                || _portalTexture.height != _portalRenderTexture.height)
            {
                if (!_portalTexture.Reinitialize(_portalRenderTexture.width,
                        _portalRenderTexture.height))
                {
                    return;
                }
            }

            Graphics.CopyTexture(_portalRenderTexture, _portalTexture);
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

            if (_portalTexture != null)
            {
                Destroy(_portalTexture);
                _portalTexture = null;
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

            _portalCamera = go.AddComponent<Camera>();
            _portalCamera.enabled = false;

            _portalCamera.depth = xrCamera.depth - 1;
            _portalCamera.cullingMask = 1 << _portalLayer | 1 << _occluderLayer;
            _portalCamera.clearFlags = CameraClearFlags.Color;
            _portalCamera.backgroundColor = Color.clear;

            _xrCamera = xrCamera;

            CopyLimitedSettingsToCamera(_xrCamera, _portalCamera);

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

            if (_portalTexture == null)
            {
                _portalTexture = new Texture2D(descriptor.width, descriptor.height,
                    descriptor.graphicsFormat, 1, TextureCreationFlags.None)
                {
                    name = "Portal Texture",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            _isInitialized = true;
        }


        private static void CopyLimitedSettingsToCamera(Camera source, Camera destination)
        {
            var scene = destination.scene;
            var cullingMask = destination.cullingMask;
            var clearFlags = destination.clearFlags;
            var backgroundColor = destination.backgroundColor;
            var depth = destination.depth;
            var targetTexture = destination.targetTexture;

            destination.CopyFrom(source);
            destination.projectionMatrix = source.projectionMatrix;

            destination.scene = scene;
            destination.cullingMask = cullingMask;
            destination.clearFlags = clearFlags;
            destination.backgroundColor = backgroundColor;
            destination.depth = depth;
            destination.targetTexture = targetTexture;
        }
    }
}

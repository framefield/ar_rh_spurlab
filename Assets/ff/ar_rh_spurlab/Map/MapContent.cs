using UnityEngine;

namespace ff.ar_rh_spurlab.Map
{
    public class MapContent : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private Camera _mapCamera;


        public RenderTexture RenderTexture => GetRenderTexture();


        private RenderTexture GetRenderTexture()
        {
            if (!_renderTexture)
            {
                _renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                _mapCamera.targetTexture = _renderTexture;
            }

            return _renderTexture;
        }

        public void SetVisibility(bool isVisible)
        {
            _mapCamera.enabled = isVisible;
            // todo disable all renderers and colliders
        }


        private RenderTexture _renderTexture;
    }
}
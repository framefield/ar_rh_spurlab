using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.LineBuildup
{
    public class BuildupLineRendererProcedural : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private PointList _pointList = null;

        [SerializeField]
        private Material _baseMaterial = null;

        #endregion


        #region Unity Callbacks

        private void Start()
        {
            if (!_pointList)
            {
                Debug.LogError("No point list assigned to BuildupLineRendererProcedural", this);
                enabled = false;
                return;
            }
            
            if (!_baseMaterial) {
                Debug.LogError("No base material assigned to BuildupLineRendererProcedural", this);
                enabled = false;
                return;
            }
            
            _pointsBuffer = new ComputeBuffer( _pointList.Points.Length, 8 * sizeof(float));
            _pointList.Fill(_pointsBuffer);
        }

        private void OnDestroy()
        {
            _pointsBuffer.Dispose();
        }

        private void Update()
        {
            if (_sharedMaterial == null)
            {
                _sharedMaterial = new Material(_baseMaterial)
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
                _materialPropertyBlock.SetBuffer(PointsPropId, _pointsBuffer);
            }
            
            // apply changes from base material
#if UNITY_EDITOR
            _materialPropertyBlock.SetColor("MainColor", _baseMaterial.GetColor("MainColor"));
            var texture = _baseMaterial.GetTexture("MainTex");
            if (texture)
            {
                _materialPropertyBlock.SetTexture("MainTex", texture);
            }
            _materialPropertyBlock.SetFloat("LineWidth", _baseMaterial.GetFloat("LineWidth"));
            _materialPropertyBlock.SetFloat("ShrinkWithDistance", _baseMaterial.GetFloat("ShrinkWithDistance"));
            _materialPropertyBlock.SetFloat("TransitionProgress", _baseMaterial.GetFloat("TransitionProgress"));
            _materialPropertyBlock.SetFloat("VisibleRange", _baseMaterial.GetFloat("VisibleRange"));
            _materialPropertyBlock.SetFloat("FogDistance", _baseMaterial.GetFloat("FogDistance"));
            _materialPropertyBlock.SetFloat("FogBias", _baseMaterial.GetFloat("FogBias"));
            _materialPropertyBlock.SetColor("FogColor", _baseMaterial.GetColor("FogColor"));
#endif

            var localTransform = transform;
            Graphics.DrawProcedural(
                _sharedMaterial,
                // TODO take correct bounds
                new Bounds(localTransform.position, localTransform.lossyScale * 500),
                MeshTopology.Points, 
                _pointList.Points.Length,
                1, 
                null, 
                _materialPropertyBlock, 
                ShadowCastingMode.Off, 
                false, 
                gameObject.layer
            );
        }

        #endregion
        
        #region Private members

        private ComputeBuffer _pointsBuffer;
        private static Material _sharedMaterial;
        
        private static readonly int PointsPropId = Shader.PropertyToID("Points");
        private MaterialPropertyBlock _materialPropertyBlock;

        #endregion
    }
}

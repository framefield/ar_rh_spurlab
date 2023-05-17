using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [ExecuteInEditMode]
    public class BuildupLineRendererProcedural : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private PointList _pointList = null;

        [SerializeField]
        private Material _baseMaterial = null;

        [SerializeField]
        [Range(0, 1)]
        private float _transitionProgress = 1;

        #endregion

        #region Initialization & Teardown

        private void Initialize()
        {
            if (!_pointList && Application.isPlaying)
            {
                Debug.LogError("No point list assigned to BuildupLineRendererProcedural", this);
                enabled = false;
                return;
            }

            if (!_baseMaterial && Application.isPlaying)
            {
                Debug.LogError("No base material assigned to BuildupLineRendererProcedural", this);
                enabled = false;
                return;
            }


            if (_pointsBuffer == null)
            {
                _pointsBuffer = new ComputeBuffer(_pointList.Points.Length, 8 * sizeof(float));
                _pointList.Fill(_pointsBuffer);
            }

            if (!_sharedMaterial)
            {
                _sharedMaterial = new Material(_baseMaterial)
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }
        }

        [ContextMenu("Reset")]
        private void TearDown()
        {
            _pointsBuffer?.Dispose();
            _pointsBuffer = null;

            if (_materialPropertyBlock != null)
            {
                _materialPropertyBlock.Clear();
                _materialPropertyBlock = null;
            }

            if (_sharedMaterial)
            {
                if (Application.isPlaying)
                {
                    Destroy(_sharedMaterial);
                }
                else
                {
                    DestroyImmediate(_sharedMaterial);
                }

                _sharedMaterial = null;
            }
        }

        #endregion


        #region Unity Callbacks

        private void Reset()
        {
            TearDown();
        }

        private void OnDestroy()
        {
            TearDown();
        }

        private void Update()
        {
            if (!IsFullyInitialized)
            {
                Initialize();
            }

#if UNITY_EDITOR
            UpdateFromBaseMaterial();
#endif
            _materialPropertyBlock.SetBuffer(PointsPropId, _pointsBuffer);
            _materialPropertyBlock.SetFloat(TransitionProgressPropId, _transitionProgress);

            var localTransform = transform;
            Graphics.DrawProcedural(
                _sharedMaterial,
                _pointList.Bounds,
                MeshTopology.Triangles,
                _pointList.Points.Length * 6 - 6,
                1,
                null,
                _materialPropertyBlock,
                ShadowCastingMode.Off,
                false,
                gameObject.layer
            );
        }

#if UNITY_EDITOR

        void OnDrawGizmosSelected()
        {
            if (!_pointList)
            {
                return;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_pointList.Bounds.center, _pointList.Bounds.size);
        }

        public bool HasFrameBounds()
        {
            return true;
        }

        public Bounds OnGetFrameBounds()
        {
            if (_pointList)
            {
                return _pointList.Bounds;
            }

            return new Bounds();
        }


        private void OnValidate()
        {
            if (_pointsBuffer == null || _pointList.Points.Length != _pointsBuffer.count)
            {
                TearDown();
            }
        }
#endif

        private void UpdateFromBaseMaterial()
        {
            if (_materialPropertyBlock == null)
            {
                return;
            }

            _materialPropertyBlock.SetColor(MainColorPropId, _baseMaterial.GetColor(MainColorPropId));
            var texture = _baseMaterial.GetTexture(MainTexPropId);
            if (texture)
            {
                _materialPropertyBlock.SetTexture(MainTexPropId, texture);
            }

            _materialPropertyBlock.SetFloat(LineWidthPropId, _baseMaterial.GetFloat(LineWidthPropId));
            _materialPropertyBlock.SetFloat(ShrinkWithDistancePropId, _baseMaterial.GetFloat(ShrinkWithDistancePropId));
            _materialPropertyBlock.SetFloat(TransitionProgressPropId, _baseMaterial.GetFloat(TransitionProgressPropId));
            _materialPropertyBlock.SetFloat(VisibleRangePropId, _baseMaterial.GetFloat(VisibleRangePropId));
            _materialPropertyBlock.SetFloat(FogDistancePropId, _baseMaterial.GetFloat(FogDistancePropId));
            _materialPropertyBlock.SetFloat(FogBiasPropId, _baseMaterial.GetFloat(FogBiasPropId));
            _materialPropertyBlock.SetColor(FogColorPropId, _baseMaterial.GetColor(FogColorPropId));
        }

        #endregion

        #region Private members

        private bool IsFullyInitialized => _materialPropertyBlock != null && _pointsBuffer != null && _sharedMaterial;

        [CanBeNull]
        private ComputeBuffer _pointsBuffer;

        [CanBeNull]
        private MaterialPropertyBlock _materialPropertyBlock;

        [CanBeNull]
        private static Material _sharedMaterial;

        private static readonly int PointsPropId = Shader.PropertyToID("Points");
        private static readonly int MainColorPropId = Shader.PropertyToID("MainColor");
        private static readonly int MainTexPropId = Shader.PropertyToID("MainTex");
        private static readonly int LineWidthPropId = Shader.PropertyToID("LineWidth");
        private static readonly int ShrinkWithDistancePropId = Shader.PropertyToID("ShrinkWithDistance");
        private static readonly int TransitionProgressPropId = Shader.PropertyToID("TransitionProgress");
        private static readonly int VisibleRangePropId = Shader.PropertyToID("VisibleRange");
        private static readonly int FogDistancePropId = Shader.PropertyToID("FogDistance");
        private static readonly int FogBiasPropId = Shader.PropertyToID("FogBias");
        private static readonly int FogColorPropId = Shader.PropertyToID("FogColor");

        #endregion
    }
}

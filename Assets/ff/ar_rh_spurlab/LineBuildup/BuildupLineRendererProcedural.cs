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
        [Range(0, 2)]
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

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui += DrawEditorScene;
#endif
        }

#if UNITY_EDITOR
        private void DrawEditorScene(UnityEditor.SceneView view)
        {
            if (view.orthographic)
            {
                return;
            }

            Draw(view.camera);
        }
#endif


        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui -= DrawEditorScene;
#endif
        }


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
            _materialPropertyBlock.SetInteger(SegmentCount, _pointList.Points.Length);
            _materialPropertyBlock.SetFloat(TransitionProgressPropId, _transitionProgress);
            // we have to build our own mvp in the shader, draw procedural does not care 
            // https://forum.unity.com/threads/how-to-get-model-matrix-into-graphics-drawprocedural-custom-shader.489304/#post-3535125
            _materialPropertyBlock.SetMatrix(ObjectToWorldPropId, transform.localToWorldMatrix);

            Draw();
        }

        private void Draw(Camera camera = null)
        {
            Graphics.DrawProcedural(
                _sharedMaterial,
                _pointList.Bounds,
                MeshTopology.Triangles,
                _pointList.Points.Length * 6 - 6,
                1,
                camera,
                _materialPropertyBlock,
                ShadowCastingMode.Off,
                false,
                gameObject.layer
            );
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
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

            _materialPropertyBlock.SetFloat(NoiseAmountId, _baseMaterial.GetFloat(NoiseAmountId));
            _materialPropertyBlock.SetFloat(NoiseVariationId, _baseMaterial.GetFloat(NoiseVariationId));
            _materialPropertyBlock.SetFloat(NoiseFrequencyId, _baseMaterial.GetFloat(NoiseFrequencyId));
            _materialPropertyBlock.SetFloat(NoisePhaseId, _baseMaterial.GetFloat(NoisePhaseId));
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
        private static readonly int SegmentCount = Shader.PropertyToID("SegmentCount");
        private static readonly int MainColorPropId = Shader.PropertyToID("MainColor");
        private static readonly int MainTexPropId = Shader.PropertyToID("MainTex");
        private static readonly int LineWidthPropId = Shader.PropertyToID("LineWidth");
        private static readonly int ShrinkWithDistancePropId = Shader.PropertyToID("ShrinkWithDistance");
        private static readonly int TransitionProgressPropId = Shader.PropertyToID("TransitionProgress");
        private static readonly int VisibleRangePropId = Shader.PropertyToID("VisibleRange");
        private static readonly int FogDistancePropId = Shader.PropertyToID("FogDistance");
        private static readonly int FogBiasPropId = Shader.PropertyToID("FogBias");
        private static readonly int FogColorPropId = Shader.PropertyToID("FogColor");
        private static readonly int ObjectToWorldPropId = Shader.PropertyToID("_ObjectToWorld");
        private static readonly int NoiseAmountId = Shader.PropertyToID("NoiseAmount");
        private static readonly int NoiseVariationId = Shader.PropertyToID("NoiseVariation");
        private static readonly int NoiseFrequencyId = Shader.PropertyToID("NoiseFrequency");
        private static readonly int NoisePhaseId = Shader.PropertyToID("NoisePhase");

        #endregion
    }
}

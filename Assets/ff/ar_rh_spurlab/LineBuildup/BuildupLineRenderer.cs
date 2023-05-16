using UnityEngine;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class BuildupLineRenderer : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] 
        [Range(1, 100)]
        private uint _interpolationSteps = 1;

        [SerializeField]
        private int _triangleBudget = 65536;
        
        [SerializeField]
        private PointList _pointList = null;

        #endregion
        
        #region Project References
        [SerializeField, HideInInspector] ComputeShader _lineRendererCompute = null;
        #endregion


        #region Unity Callbacks

        private void Start()
        {
            if (!_pointList)
            {
                Debug.LogError("No point list assigned to BuildupLineRenderer", this);
                enabled = false;
                return;
            }
            
            _pointsBuffer = new ComputeBuffer(_pointList.Points.Length, 8 * sizeof(float));
            _pointsBuffer.SetData(_pointList.Points);
            _meshFilter = GetComponent<MeshFilter>();
            _builder = new MeshBuilder(_triangleBudget);
        }

        private void OnDestroy()
        {
            _pointsBuffer.Dispose();
            _builder.Dispose();
        }

        private void Update()
        {
            _lineRendererCompute.SetFloat("p_time", Time.time);
            _lineRendererCompute.SetFloat("p_max_triangle_count", _triangleBudget);
            _lineRendererCompute.SetUInt("p_interpolation_steps", _interpolationSteps);
            
            _lineRendererCompute.SetBuffer(0, "i_points", _pointsBuffer);
            
            _builder.BuildMeshByCompute(_lineRendererCompute);
            
            _meshFilter.sharedMesh = _builder.Mesh;
        }

        #endregion
        
        #region Private members

        private ComputeBuffer _pointsBuffer;
        private MeshFilter _meshFilter;
        private MeshBuilder _builder;

        #endregion
    }
}

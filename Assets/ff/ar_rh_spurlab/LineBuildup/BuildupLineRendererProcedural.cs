using UnityEngine;
using UnityEngine.Rendering;

namespace ff.ar_rh_spurlab.LineBuildup
{
    public class BuildupLineRendererProcedural : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private PointList _pointList = null;
        
        [SerializeField]
        private Shader _shader = null;

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
            
            if (!_shader) {
                Debug.LogError("No shader assigned to BuildupLineRendererProcedural", this);
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
            if (_material == null || _material.shader != _shader)
            {
                _material = new Material(_shader)
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            _material.SetBuffer(PointsPropId, _pointsBuffer);
            
            var localTransform = transform;
            Graphics.DrawProcedural(
                _material,
                new Bounds(localTransform.position, localTransform.lossyScale * 5),
                MeshTopology.Points, _pointList.Points.Length
            );
        }

        #endregion
        
        #region Private members

        private ComputeBuffer _pointsBuffer;
        private Material _material;
        
        private static readonly int PointsPropId = Shader.PropertyToID("Points");

        #endregion
    }
}

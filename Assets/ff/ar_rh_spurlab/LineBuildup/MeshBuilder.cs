using UnityEngine;
using UnityEngine.Rendering;

namespace ff.ar_rh_spurlab.LineBuildup
{
    public class MeshBuilder : System.IDisposable
    {
        public Mesh Mesh => _mesh;
        
        public MeshBuilder(int triangleBudget)
        {
            // local buffers
            // Buffer for triangle counting
            _counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
            
            AllocateMesh(3 * triangleBudget);
        }


        public void BuildMeshByCompute(ComputeShader compute)
        {
            _counterBuffer.SetCounterValue(0);
            
            // build mesh
            compute.SetBuffer(0, "o_vertex_buffer", _vertexBuffer);
            compute.SetBuffer(0, "o_index_buffer", _indexBuffer);
            compute.SetBuffer(0, "o_counter", _counterBuffer);
            compute.DispatchThreads(0, 1, 1, 1);
            
            
            // Clear unused area of the buffers.
            compute.SetBuffer(1, "o_vertex_buffer", _vertexBuffer);
            compute.SetBuffer(1, "o_index_buffer", _indexBuffer);
            compute.SetBuffer(1, "o_counter", _counterBuffer);
            compute.DispatchThreads(1, 1024, 1, 1);
            
            // TODO mesh bounds?
        }
        
        private void AllocateMesh(int vertexCount)
        {
            _mesh = new Mesh();

            // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
            _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
            _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

            // Vertex position: float32 x 3
            var vp = new VertexAttributeDescriptor
                (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

            // Vertex normal: float32 x 3
            var vn = new VertexAttributeDescriptor
                (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

            // Vertex/index buffer formats
            _mesh.SetVertexBufferParams(vertexCount, vp, vn);
            _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

            // Sub-mesh initialization
            _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount),
                MeshUpdateFlags.DontRecalculateBounds);

            // GraphicsBuffer references
            _vertexBuffer = _mesh.GetVertexBuffer(0);
            _indexBuffer = _mesh.GetIndexBuffer();
        }

        public void Dispose()
        {
            // local buffers
            _counterBuffer.Dispose();
            // mesh
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            Object.Destroy(_mesh);
        }
        

        // local buffers
        private readonly ComputeBuffer _counterBuffer;
        
        // mesh
        private Mesh _mesh;
        private GraphicsBuffer _vertexBuffer;
        private GraphicsBuffer _indexBuffer;

        
    }
}

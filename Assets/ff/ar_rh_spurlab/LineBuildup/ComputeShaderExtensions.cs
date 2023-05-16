using UnityEngine;

namespace ff.ar_rh_spurlab.LineBuildup
{
    static class ComputeShaderExtensions
    {
        
        public static void SetUInt(this ComputeShader compute, string nameID, uint val) => compute.SetInt(nameID, (int)val);
        
        public static void DispatchThreads
            (this ComputeShader compute, int kernel, int x, int y, int z)
        {
            compute.GetKernelThreadGroupSizes(kernel, out var xc, out var yc, out var zc);

            x = (x + (int)xc - 1) / (int)xc;
            y = (y + (int)yc - 1) / (int)yc;
            z = (z + (int)zc - 1) / (int)zc;

            compute.Dispatch(kernel, x, y, z);
        }

        public static void DispatchThreads
            (this ComputeShader compute, int kernel, (int x, int y, int z) t)
            => DispatchThreads(compute, kernel, t.x, t.y, t.z);

        public static void DispatchThreads
            (this ComputeShader compute, int kernel, Vector3Int v)
            => DispatchThreads(compute, kernel, v.x, v.y, v.z);
    }
}

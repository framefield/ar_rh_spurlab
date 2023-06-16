// code is based on: https://gist.github.com/yasirkula/d09bbc1e16dc96354b2e7162b351f964

using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CircleGraphic : MaskableGraphic
    {
        [SerializeField]
        private int _detail = 64;

        [SerializeField]
        private float _edgeThickness = 1;


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();

            var width = r.width * 0.5f;
            var height = r.height * 0.5f;

            vh.Clear();

            var pivot = rectTransform.pivot;
            var deltaWidth = r.width * (0.5f - pivot.x);
            var deltaHeight = r.height * (0.5f - pivot.y);

            var deltaRadians = 360f / _detail * Mathf.Deg2Rad;

            var innerWidth = width - _edgeThickness;
            var innerHeight = height - _edgeThickness;
            var uv = Vector2.zero;

            vh.AddVert(new Vector3(width + deltaWidth, deltaHeight, 0f), color, uv);
            vh.AddVert(new Vector3(innerWidth + deltaWidth, deltaHeight, 0f), color, uv);


            var triangleIndex = 2;
            for (var i = 1; i < _detail; i++, triangleIndex += 2)
            {
                var radians = i * deltaRadians;
                var cos = Mathf.Cos(radians);
                var sin = Mathf.Sin(radians);


                vh.AddVert(new Vector3(cos * width + deltaWidth, sin * height + deltaHeight, 0f), color, uv);
                vh.AddVert(new Vector3(cos * innerWidth + deltaWidth, sin * innerHeight + deltaHeight, 0f),
                    color,
                    uv);

                vh.AddTriangle(triangleIndex, triangleIndex - 2, triangleIndex - 1);
                vh.AddTriangle(triangleIndex, triangleIndex - 1, triangleIndex + 1);
            }

            vh.AddTriangle(0, triangleIndex - 2, triangleIndex - 1);
            vh.AddTriangle(0, triangleIndex - 1, 1);
        }
    }
}
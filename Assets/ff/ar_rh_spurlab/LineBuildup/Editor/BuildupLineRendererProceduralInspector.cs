using System;
using UnityEditor;
using UnityEngine;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [CustomEditor(typeof(BuildupLineRendererProcedural))]
    public class BuildupLineRendererProceduralInspector : Editor
    {
        private void OnSceneGUI()
        {
            var lineRenderer = (BuildupLineRendererProcedural)target;
            var pointList = lineRenderer.PointList;
            if (pointList.Points.Length < 500)
            {
                var localToWorldMatrix = lineRenderer.transform.localToWorldMatrix;
                var labelStyle = new GUIStyle("MiniLabel");
                labelStyle.normal.textColor = Color.cyan;
                for (var index = 0; index < pointList.Points.Length; index++)
                {
                    var point = pointList.Points[index];
                    if (float.IsNaN(point.W))
                    {
                        continue;
                    }

                    var pos = localToWorldMatrix.MultiplyPoint(point.Position);
                    pos += Vector3.up * 0.2f;
                    Handles.Label(pos, $"#{index}", labelStyle);
                }
            }
        }
    }
}

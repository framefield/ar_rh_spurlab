using UnityEditor;
using UnityEngine;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [CustomEditor(typeof(PointList))]
    public class PointsInspector : Editor
    {
        private string _lineStats = null;

        private void OnEnable()
        {
            var pointList = (PointList)target;
            if (pointList)
            {
                var stringBuilder = new System.Text.StringBuilder();
                var lineNo = 0;
                var lineLength = 0;
                foreach (var point in pointList.Points)
                {
                    if (float.IsNaN(point.W))
                    {
                        stringBuilder.AppendLine($"#{lineNo}: {lineLength}");
                        lineNo++;
                        lineLength = 0;
                    }
                    else
                    {
                        lineLength++;
                    }
                }

                _lineStats = stringBuilder.ToString();
            }
        }

        private void OnDisable()
        {
            _lineStats = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Statistics", EditorStyles.boldLabel);

                var pointList = (PointList)target;
                GUILayout.Label($"Points: {pointList.Points.Length}");


                if (!string.IsNullOrEmpty(_lineStats))
                {
                    EditorGUILayout.HelpBox(_lineStats, MessageType.Info);
                }
            }
        }
    }
}

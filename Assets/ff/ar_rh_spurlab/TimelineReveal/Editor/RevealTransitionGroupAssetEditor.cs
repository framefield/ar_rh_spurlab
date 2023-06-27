using System;
using UnityEditor;
using UnityEngine;

namespace ff.ar_rh_spurlab.TimelineReveal.Editor
{
    [CustomEditor(typeof(RevealTransitionGroupAsset))]
    public class RevealTransitionGroupAssetEditor : UnityEditor.Editor
    {
        private bool _showActiveLines = false;

        private void OnEnable()
        {
            if (target is not RevealTransitionGroupAsset asset)
            {
                return;
            }

            asset.UpdateDefinitions();
            EditorUtility.SetDirty(asset);
        }


        public override void OnInspectorGUI()
        {
            var changed = DrawDefaultInspector();
            if (target is not RevealTransitionGroupAsset asset)
            {
                return;
            }

            if (changed)
            {
                asset.UpdateDefinitions();
                EditorUtility.SetDirty(asset);
            }

            var stats = asset.GetActiveInfoStats();
            _showActiveLines =
                EditorGUILayout.Foldout(_showActiveLines, $"Active Lines ({stats.ActiveCount}/{stats.Count})", true);
            if (_showActiveLines)
            {
                GUILayout.Label(stats.ActiveNames);
            }

            GUI.enabled = !Application.isPlaying;

            if (GUILayout.Button("Activate All"))
            {
                asset.SetActiveAll(true);
            }

            if (GUILayout.Button("Deactivate All"))
            {
                asset.SetActiveAll(false);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace ff.ar_rh_spurlab.TimelineReveal.Editor
{
    [CustomEditor(typeof(RevealTransitionGroupAsset))]
    public class RevealTransitionGroupAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (target is not RevealTransitionGroupAsset asset) return;

            GUILayout.Label($"Active Lines:\n{asset.GetActiveInfoText()}");

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
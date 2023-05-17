using UnityEditor;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations.Editor
{
    [CustomEditor(typeof(LocationTimelineManager))]
    public class LocationTimelineManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (target is not LocationTimelineManager locationTimelineManager)
                    return;

                if (GUILayout.Button("Play Chapters"))
                {
                    locationTimelineManager.PlayChapters();
                }
            }
        }
    }
}
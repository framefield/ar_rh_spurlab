using UnityEditor;

namespace ff.ar_rh_spurlab.Locations.Editor
{
    [CustomEditor(typeof(AugmentedLocation))]
    public class LocationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.HelpBox(
                "\nCollects all components that implement ITrackedLocationContent. Forwards tracking events to these components.\n\nExample components that implement ITrackedLocationContent: SimpleTrackedLocationContent, LocationTimelineManager.\n",
                MessageType.Info);
        }
    }
}
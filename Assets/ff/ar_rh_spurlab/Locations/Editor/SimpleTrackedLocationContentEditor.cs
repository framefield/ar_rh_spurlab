using UnityEditor;

namespace ff.ar_rh_spurlab.Locations.Editor
{
    [CustomEditor(typeof(SimpleTrackedLocationContent))]
    public class SimpleTrackedLocationContentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.HelpBox(
                "\nThis components collects all the children renderers, colliders and canvas.\n\nWhen location is not tracked anymore stores all the enabled states and disables these components. When location is tracked again all the component states are restored.\n",
                MessageType.Info);
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace ff.ar_rh_spurlab.Positioning
{
    [CustomEditor(typeof(PositioningService))]
    public class LocationEditor : Editor
    {
        private GeoPosition _positionOverride;
        private double _headingOverride;
        private float _verticalAccuracy;
        private float _horizontalAccuracy;
        private float _headingAccuracy;

        private bool _applyContinuously = true;

        private void OnEnable()
        {
            _headingOverride = EditorPrefs.GetFloat("PositioningServiceEditor.Heading", 0f);
            _positionOverride.Latitude = EditorPrefs.GetFloat("PositioningServiceEditor.Latitude", 52.54087049633316f);
            _positionOverride.Longitude =
                EditorPrefs.GetFloat("PositioningServiceEditor.Longitude", 13.386393768213711f);
            _verticalAccuracy = EditorPrefs.GetFloat("PositioningServiceEditor.VerticalAccuracy", 0f);
            _horizontalAccuracy = EditorPrefs.GetFloat("PositioningServiceEditor.HorizontalAccuracy", 0f);
            _headingAccuracy = EditorPrefs.GetFloat("PositioningServiceEditor.HeadingAccuracy", 0f);
            _applyContinuously = EditorPrefs.GetBool("PositioningServiceEditor.ApplyContinuously", true);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            var positioningService = (PositioningService)target;
            GUILayout.Label("Position Override", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUI.BeginChangeCheck();
                _positionOverride.Latitude = EditorGUILayout.DoubleField("Latitude", _positionOverride.Latitude);
                _positionOverride.Longitude = EditorGUILayout.DoubleField("Longitude", _positionOverride.Longitude);
                _verticalAccuracy = EditorGUILayout.FloatField("Vertical Accuracy", _verticalAccuracy);
                _horizontalAccuracy = EditorGUILayout.FloatField("Horizontal Accuracy", _horizontalAccuracy);
                _headingOverride = EditorGUILayout.DoubleField("Heading", _headingOverride);
                _headingAccuracy = EditorGUILayout.FloatField("Heading Accuracy", _headingAccuracy);
                _applyContinuously = EditorGUILayout.Toggle("Apply continuously", _applyContinuously);
                var changed = EditorGUI.EndChangeCheck();

                if (changed)
                {
                    EditorPrefs.SetFloat("PositioningServiceEditor.Latitude", (float)_positionOverride.Latitude);
                    EditorPrefs.SetFloat("PositioningServiceEditor.Longitude", (float)_positionOverride.Longitude);
                    EditorPrefs.SetFloat("PositioningServiceEditor.Heading", (float)_headingOverride);
                    EditorPrefs.SetFloat("PositioningServiceEditor.VerticalAccuracy", _verticalAccuracy);
                    EditorPrefs.SetFloat("PositioningServiceEditor.HorizontalAccuracy", _horizontalAccuracy);
                    EditorPrefs.SetFloat("PositioningServiceEditor.HeadingAccuracy", _headingAccuracy);
                    EditorPrefs.SetBool("PositioningServiceEditor.ApplyContinuously", _applyContinuously);

                    if (_applyContinuously)
                    {
                        positioningService.OverridePosition(_positionOverride, _headingOverride,
                            _verticalAccuracy,
                            _horizontalAccuracy,
                            _headingAccuracy
                        );
                    }
                }


                if (GUILayout.Button("Apply Override"))
                {
                    positioningService.OverridePosition(_positionOverride, _headingOverride,
                        _verticalAccuracy,
                        _horizontalAccuracy,
                        _headingAccuracy
                    );
                }
            }
        }
    }
}

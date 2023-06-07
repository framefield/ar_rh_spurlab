using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

namespace ff.ar_rh_spurlab.LineBuildup
{
    [Serializable]
    public class PointListImport
    {
        public Point[] StructuredList;
    }

    [ScriptedImporter(2, "points")]
    public class PointsImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var points = JsonConvert.DeserializeObject<PointListImport>(File.ReadAllText(ctx.assetPath));
            var pointList = ScriptableObject.CreateInstance<PointList>();

            pointList.Points = points.StructuredList;
            pointList.GenerateBounds();


            ctx.AddObjectToAsset("pointList", pointList);
            ctx.SetMainObject(pointList);
        }
    }

    [CustomEditor(typeof(PointsImporter))]
    public class PointsImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Reimport"))
            {
                var importer = (PointsImporter)target;
                importer.SaveAndReimport();
            }
        }
    }
}

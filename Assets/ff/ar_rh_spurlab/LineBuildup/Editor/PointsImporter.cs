using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

namespace ff.ar_rh_spurlab.LineBuildup.Editor
{
    [Serializable]
    public class PointListImport
    {
        public Point[] StructuredList;
    }
    
    [ScriptedImporter(1, "points")]
    public class PointsImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var points = JsonUtility.FromJson<PointListImport>(File.ReadAllText(ctx.assetPath));
            var pointList = ScriptableObject.CreateInstance<PointList>();
            
            pointList.Points = points.StructuredList;
            
            ctx.AddObjectToAsset("pointList", pointList);
            ctx.SetMainObject(pointList);
        }
    }
}

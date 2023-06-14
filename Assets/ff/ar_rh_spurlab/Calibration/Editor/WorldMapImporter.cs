using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace ff.ar_rh_spurlab.Calibration
{
    [ScriptedImporter(1, "worldmap")]
    public class WorldMapImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var worldMap = ScriptableObject.CreateInstance<WorldMap>();
            worldMap.Bytes = File.ReadAllBytes(ctx.assetPath);

            ctx.AddObjectToAsset("worldMap", worldMap);
            ctx.SetMainObject(worldMap);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace ff.ar_rh_spurlab.AR
{
    public static class ARWorldMapController
    {
        private static bool IsSupported(ARSession arSession)
        {
#if UNITY_IOS
            return arSession.subsystem is ARKitSessionSubsystem && ARKitSessionSubsystem.worldMapSupported;
#else
                return false;
#endif
        }


#if UNITY_IOS
        public static IEnumerator Save(ARSession arSession, string worldMapFilePath)
        {
            if (!IsSupported(arSession))
            {
                yield break;
            }

            var sessionSubsystem = (ARKitSessionSubsystem)arSession.subsystem;
            if (sessionSubsystem == null)
            {
                Debug.Log("No session subsystem available. Could not save.");
                yield break;
            }

            var request = sessionSubsystem.GetARWorldMapAsync();

            while (!request.status.IsDone())
                yield return null;

            if (request.status.IsError())
            {
                Debug.Log($"Session serialization failed with status {request.status}");
                yield break;
            }

            var worldMap = request.GetWorldMap();
            request.Dispose();

            SaveAndDisposeWorldMap(worldMap, worldMapFilePath);
        }

        private static void SaveAndDisposeWorldMap(ARWorldMap worldMap, string worldMapFilePath)
        {
            Debug.Log("Serializing ARWorldMap to byte array...");
            var data = worldMap.Serialize(Allocator.Temp);
            Debug.Log($"ARWorldMap has {data.Length} bytes.");

            var file = File.Open(worldMapFilePath, FileMode.Create);
            var writer = new BinaryWriter(file);
            writer.Write(data.ToArray());
            writer.Close();
            data.Dispose();
            worldMap.Dispose();
            Debug.Log($"ARWorldMap written to {worldMapFilePath}");
        }

        public static IEnumerator Load(ARSession arSession, string worldMapFilePath)
        {
            if (!IsSupported(arSession))
            {
                yield break;
            }

            var sessionSubsystem = (ARKitSessionSubsystem)arSession.subsystem;
            if (sessionSubsystem == null)
            {
                Debug.Log("No session subsystem available. Could not load.");
                yield break;
            }

            FileStream file;
            try
            {
                file = File.Open(worldMapFilePath, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                Debug.LogError(
                    $"No ARWorldMap was found in {worldMapFilePath}. Make sure to save the ARWorldMap before attempting to load it.");
                yield break;
            }

            Debug.Log($"Reading {worldMapFilePath}...");

            const int bytesPerFrame = 1024 * 10;
            var bytesRemaining = file.Length;
            var binaryReader = new BinaryReader(file);
            var allBytes = new List<byte>();
            while (bytesRemaining > 0)
            {
                var bytes = binaryReader.ReadBytes(bytesPerFrame);
                allBytes.AddRange(bytes);
                bytesRemaining -= bytesPerFrame;
                yield return null;
            }

            var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
            data.CopyFrom(allBytes.ToArray());

            Debug.Log("Deserializing to ARWorldMap...");
            if (ARWorldMap.TryDeserialize(data, out var worldMap))
            {
                data.Dispose();
            }

            if (worldMap.valid)
            {
                Debug.Log("Deserialized successfully.");
            }
            else
            {
                Debug.LogError("Data is not a valid ARWorldMap.");
                yield break;
            }

            Debug.Log("Apply ARWorldMap to current session.");
            sessionSubsystem.ApplyWorldMap(worldMap);
        }
#endif
    }
}
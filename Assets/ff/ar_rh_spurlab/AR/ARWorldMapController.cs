using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

#if UNITY_IOS
using System;
using UnityEngine.XR.ARKit;
#endif

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
        public static async Task Save(ARSession arSession, string worldMapFilePath)
        {
            if (!IsSupported(arSession))
            {
                return;
            }

            var sessionSubsystem = (ARKitSessionSubsystem)arSession.subsystem;
            if (sessionSubsystem == null)
            {
                Debug.Log("No session subsystem available. Could not save.");
                return;
            }

            using var request = sessionSubsystem.GetARWorldMapAsync();
            while (!request.status.IsDone())
            {
                await Task.Delay(100);
            }

            if (request.status.IsError())
            {
                Debug.Log($"Session serialization failed with status {request.status}");
                return;
            }

            await SaveWorldMap(request, worldMapFilePath);
        }

        private static async Task SaveWorldMap(ARWorldMapRequest request, string worldMapFilePath)
        {
            using var worldMap = request.GetWorldMap();
            Debug.Log("Serializing ARWorldMap to byte array...");
            var size = 0;
            await Task.Factory.StartNew(() =>
            {
                using var data = worldMap.Serialize(Allocator.Temp);
                size = data.Length;
                var file = File.Open(worldMapFilePath, FileMode.Create);
                using var writer = new BinaryWriter(file);
                writer.Write(data.ToArray());
                writer.Close();
            });
            Debug.Log($"ARWorldMap ({size} Bytes) written to {worldMapFilePath}");
        }

        private static readonly string[] ByteSizeLabels = { "B", "KB", "MB", "GB", "TB" };

        private static string FormatByteSize(long len)
        {
            var order = 0;
            while (len >= 1024 && order < ByteSizeLabels.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:F1} {ByteSizeLabels[order]}";
        }

        public static async Task<bool> Load(ARSession arSession, string worldMapFilePath)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported(arSession))
            {
                return false;
            }

            var sessionSubsystem = (ARKitSessionSubsystem)arSession.subsystem;
            if (sessionSubsystem == null)
            {
                Debug.Log("No session subsystem available. Could not load.");
                return false;
            }
#endif

            try
            {
                var loadStartTime = DateTime.Now;

                Debug.Log($"Reading {worldMapFilePath}...");
                var worldMap = await LoadWorldMapData(worldMapFilePath);
                var loadingDuration = DateTime.Now - loadStartTime;
                Debug.Log(
                    $"Read {worldMapFilePath} ({FormatByteSize(worldMap.size)}) in {loadingDuration.TotalSeconds:F1} seconds.");

                if (worldMap.map.HasValue)
                {
                    var applyStartTime = DateTime.Now;
                    Debug.Log("Apply ARWorldMap to current session.");
#if UNITY_IOS && !UNITY_EDITOR
                    sessionSubsystem.ApplyWorldMap(worldMap.Value);
#endif
                    var applyDuration = DateTime.Now - applyStartTime;
                    Debug.Log($"Applied {worldMapFilePath} in {applyDuration.TotalSeconds:F1} seconds.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarning(
                    $"No ARWorldMap was found in {worldMapFilePath}. Make sure to save the ARWorldMap before attempting to load it.");
            }

            return false;
        }

        private static async Task<(ARWorldMap? map, long size)> LoadWorldMapData(string worldMapFilePath)
        {
            await using var file = File.Open(worldMapFilePath, FileMode.Open);

            using var binaryReader = new BinaryReader(file);
            const int bufferSize = 4096;
            NativeArray<byte> data;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = binaryReader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, count);
                }

                data = new NativeArray<byte>(ms.ToArray(), Allocator.Temp);
            }

            using (data)
            {
#if UNITY_IOS && !UNITY_EDITOR
                if (ARWorldMap.TryDeserialize(data, out var worldMap))
                {
                    data.Dispose();

                    if (!worldMap.valid)
                    {
                        throw new Exception("Data is not a valid ARWorldMap.");
                    }

                    return worldMap;
                }
#else
                return (new ARWorldMap(), data.Length);
#endif

                throw new Exception("Data is not a valid ARWorldMap.");
            }
        }
#endif
    }
}

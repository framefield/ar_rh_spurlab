using System.IO;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;

namespace ff.ar_rh_spurlab.Calibration
{
    public class PreinstallCalibrationData : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void InstallCalibration()
        {
            Debug.Log("Running PreinstallCalibrationData");
            Install();
        }

        private static void Install()
        {
            var availableSites = AvailableSites.LoadFromResources();

            if (!availableSites)
            {
                return;
            }

            foreach (var site in availableSites.Sites)
            {
                foreach (var location in site.Locations)
                {
                    var locationDataDirectory = Path.Combine(Application.persistentDataPath, location.Id);

                    var worldMapFilePath = Path.Combine(locationDataDirectory, "my_session.worldmap");
                    var calibrationDataFilePath = Path.Combine(locationDataDirectory, "calibrationdata.json");

                    Directory.CreateDirectory(locationDataDirectory);

                    if (!File.Exists(worldMapFilePath))
                    {
                        var worldMap = location.PreinstallWorldMap;
                        if (worldMap)
                        {
                            Debug.Log($"PreinstallCalibrationData - writing world map for {location.Id}");
                            var worldMapBytes = worldMap.Bytes;
                            File.WriteAllBytes(worldMapFilePath, worldMapBytes);
                        }
                    }

                    if (!File.Exists(calibrationDataFilePath))
                    {
                        var calibrationData = location.PreinstallCalibrationData;
                        if (calibrationData)
                        {
                            Debug.Log($"PreinstallCalibrationData - writing calibration data for {location.Id}");
                            var calibrationDataJson = calibrationData.text;
                            File.WriteAllText(calibrationDataFilePath, calibrationDataJson);
                        }
                    }
                }
            }
        }
    }
}

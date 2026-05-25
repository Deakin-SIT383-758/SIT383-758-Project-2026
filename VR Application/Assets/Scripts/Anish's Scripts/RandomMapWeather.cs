using UnityEngine;

public class RandomMapWeather : MonoBehaviour
{
    [System.Serializable]
    public struct ZoneWeatherValues
    {
        public int zoneNumber;
        public float cloudiness;
        public float rainStrength;
        public float windStrength;
    }

    [Header("Weather Zones")]
    public WeatherZone[] weatherZones;

    [Header("Current Zone Values")]
    public ZoneWeatherValues[] zoneValues;

    [Header("Random Settings")]
    public bool randomizeOnStart = true;

    void Start()
    {
        if (randomizeOnStart)
            RandomizeAllZones();
        else
            RefreshZoneValues();
    }

    public void RandomizeAllZones()
    {
        if (weatherZones == null || weatherZones.Length == 0)
        {
            Debug.LogWarning("No weather zones assigned.");
            return;
        }

        for (int i = 0; i < weatherZones.Length; i++)
        {
            if (weatherZones[i] != null)
                weatherZones[i].RandomizeWeather();
        }

        RefreshZoneValues();

        Debug.Log("Weather randomized.");
    }

    public void RefreshZoneValues()
    {
        if (weatherZones == null)
        {
            zoneValues = new ZoneWeatherValues[0];
            return;
        }

        zoneValues = new ZoneWeatherValues[weatherZones.Length];

        for (int i = 0; i < weatherZones.Length; i++)
        {
            WeatherZone zone = weatherZones[i];

            zoneValues[i] = new ZoneWeatherValues
            {
                zoneNumber = i + 1,
                cloudiness = zone != null ? zone.CurrentCloudiness : 0f,
                rainStrength = zone != null ? zone.CurrentRainStrength : 0f,
                windStrength = zone != null ? zone.CurrentWindStrength : 0f
            };
        }
    }

    public ZoneWeatherValues GetZoneValues(int zoneNumber)
    {
        RefreshZoneValues();

        int index = zoneNumber - 1;

        if (index < 0 || index >= zoneValues.Length)
        {
            Debug.LogWarning($"Zone {zoneNumber} does not exist.");
            return default;
        }

        return zoneValues[index];
    }

    public ZoneWeatherValues[] GetAllZoneValues()
    {
        RefreshZoneValues();
        return zoneValues;
    }

    public void PrintAllWeatherZones()
    {
        RefreshZoneValues();

        if (zoneValues == null || zoneValues.Length == 0)
        {
            Debug.LogWarning("No weather zone values available.");
            return;
        }

        foreach (var zone in zoneValues)
        {
            Debug.Log(
                $"Zone {zone.zoneNumber} | " +
                $"Cloud: {zone.cloudiness:0.00} | " +
                $"Rain: {zone.rainStrength:0.00} | " +
                $"Wind: {zone.windStrength:0.00}"
            );
        }
    }

    public void OnRandomizeWeatherButtonPressed()
    {
        RandomizeAllZones();
    }

    public void OnPrintWeatherButtonPressed()
    {
        PrintAllWeatherZones();
    }
}
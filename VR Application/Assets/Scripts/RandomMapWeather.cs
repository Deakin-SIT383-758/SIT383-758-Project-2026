using UnityEngine;

public class RandomMapWeather : MonoBehaviour
{
    [Header("Weather Zones")]
    public WeatherZone[] weatherZones;

    [Header("Random Settings")]
    public bool randomizeOnStart = true;

    void Start()
    {
        if (randomizeOnStart)
        {
            RandomizeAllZones();
        }
    }

    // Can be called from code OR a UI button
    public void RandomizeAllZones()
    {
        if (weatherZones == null || weatherZones.Length == 0)
        {
            Debug.LogWarning("No weather zones assigned.");
            return;
        }

        foreach (WeatherZone zone in weatherZones)
        {
            if (zone != null)
            {
                zone.RandomizeWeather();
            }
        }

        Debug.Log("Weather randomized.");
    }

    // Optional dedicated button method
    public void OnRandomizeWeatherButtonPressed()
    {
        RandomizeAllZones();
    }
}
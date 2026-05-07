using UnityEngine;

public class RandomMapWeather : MonoBehaviour
{
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

    public void RandomizeAllZones()
    {
        foreach (WeatherZone zone in weatherZones)
        {
            if (zone != null)
            {
                zone.RandomizeWeather();
            }
        }
    }
}
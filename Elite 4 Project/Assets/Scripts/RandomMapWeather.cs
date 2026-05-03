using UnityEngine;

public class RandomMapWeather : MonoBehaviour
{
    public WeatherZone[] weatherZones;

    [Header("Random Settings")]
    public bool randomizeOnStart = true;
    public float randomizeInterval = 10f;

    private float timer;

    void Start()
    {
        if (randomizeOnStart)
        {
            RandomizeAllZones();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= randomizeInterval)
        {
            timer = 0f;
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
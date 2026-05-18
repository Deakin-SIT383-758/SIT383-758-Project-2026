using UnityEngine;

public class WeatherReader : MonoBehaviour
{
    public RandomMapWeather randomMapWeather;

    public void PrintAllWeatherZones()
    {
        if (randomMapWeather == null)
        {
            Debug.LogWarning("RandomMapWeather not assigned.");
            return;
        }

        var allZones = randomMapWeather.GetAllZoneValues();

        foreach (var zone in allZones)
        {
            Debug.Log(
                $"Zone {zone.zoneNumber} | " +
                $"Cloud: {zone.cloudiness:0.00} | " +
                $"Rain: {zone.rainStrength:0.00} | " +
                $"Wind: {zone.windStrength:0.00}"
            );
        }
    }
}
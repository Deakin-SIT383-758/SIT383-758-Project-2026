using UnityEngine;

public class WeatherZone : MonoBehaviour
{
    public enum WeatherType
    {
        Clear,
        Cloud,
        Wind,
        CloudAndRain,
        CloudAndWind,
        RainCloudAndWind
    }

    [Header("Current Weather")]
    public WeatherType currentWeather;

    [Header("Visuals")]
    public ParticleSystem rainParticles;
    public GameObject cloudVisual;
    public ParticleSystem windParticles;

    [Header("Rain Settings")]
    public float heavyRainRate = 200f;

    [Header("Wind Settings")]
    public float windRate = 80f;

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;

        bool hasRain =
            weather == WeatherType.CloudAndRain ||
            weather == WeatherType.RainCloudAndWind;

        bool hasCloud =
            weather == WeatherType.Cloud ||
            weather == WeatherType.CloudAndRain ||
            weather == WeatherType.CloudAndWind ||
            weather == WeatherType.RainCloudAndWind;

        bool hasWind =
            weather == WeatherType.Wind ||
            weather == WeatherType.CloudAndWind ||
            weather == WeatherType.RainCloudAndWind;

        SetRain(hasRain);
        SetCloud(hasCloud);
        SetWind(hasWind);

        Debug.Log($"{gameObject.name} weather: {weather}");
    }

    void SetRain(bool active)
    {
        if (rainParticles == null) return;

        var emission = rainParticles.emission;
        emission.rateOverTime = active ? heavyRainRate : 0f;

        if (active)
        {
            rainParticles.gameObject.SetActive(true);
            rainParticles.Play();
        }
        else
        {
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rainParticles.gameObject.SetActive(false);
        }
    }

    void SetCloud(bool active)
    {
        if (cloudVisual != null)
        {
            cloudVisual.SetActive(active);
        }
    }

    void SetWind(bool active)
    {
        if (windParticles == null) return;

        var emission = windParticles.emission;
        emission.rateOverTime = active ? windRate : 0f;

        if (active)
        {
            windParticles.gameObject.SetActive(true);
            windParticles.Play();
        }
        else
        {
            windParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            windParticles.gameObject.SetActive(false);
        }
    }

    public void RandomizeWeather()
    {
        int count = System.Enum.GetValues(typeof(WeatherType)).Length;
        WeatherType randomWeather = (WeatherType)Random.Range(0, count);
        SetWeather(randomWeather);
    }
}
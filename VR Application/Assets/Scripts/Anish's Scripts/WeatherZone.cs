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

    [Header("Current Weather Values")]
    [Range(0f, 1f)] public float currentCloudiness;
    [Range(0f, 1f)] public float currentRainStrength;
    [Range(0f, 1f)] public float currentWindStrength;

    public float CurrentCloudiness => currentCloudiness;
    public float CurrentRainStrength => currentRainStrength;
    public float CurrentWindStrength => currentWindStrength;

    [Header("Visuals")]
    public ParticleSystem rainParticles;
    public GameObject cloudVisual;
    public ParticleSystem windParticles;

    [Header("Rain Strength")]
    [Range(0f, 1f)] public float minRainStrength = 0.4f;
    [Range(0f, 1f)] public float maxRainStrength = 1f;
    public float maxRainEmissionRate = 200f;

    [Header("Wind Strength")]
    [Range(0f, 1f)] public float minWindStrength = 0.4f;
    [Range(0f, 1f)] public float maxWindStrength = 1f;
    public float maxWindEmissionRate = 80f;

    [Header("Cloud Strength")]
    [Range(0f, 1f)] public float cloudStrengthWhenActive = 1f;

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

        currentCloudiness = hasCloud ? cloudStrengthWhenActive : 0f;
        currentRainStrength = hasRain ? Random.Range(minRainStrength, maxRainStrength) : 0f;
        currentWindStrength = hasWind ? Random.Range(minWindStrength, maxWindStrength) : 0f;

        SetRain(hasRain, currentRainStrength);
        SetCloud(hasCloud);
        SetWind(hasWind, currentWindStrength);
    }

    void SetRain(bool active, float strength)
    {
        if (rainParticles == null)
            return;

        var emission = rainParticles.emission;
        emission.rateOverTime = active ? maxRainEmissionRate * strength : 0f;

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
            cloudVisual.SetActive(active);
    }

    void SetWind(bool active, float strength)
    {
        if (windParticles == null)
            return;

        var emission = windParticles.emission;
        emission.rateOverTime = active ? maxWindEmissionRate * strength : 0f;

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

    public Vector3 GetWeatherValues()
    {
        return new Vector3(
            currentCloudiness,
            currentRainStrength,
            currentWindStrength
        );
    }
}
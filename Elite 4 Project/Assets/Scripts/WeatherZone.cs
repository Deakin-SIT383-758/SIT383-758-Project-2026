using UnityEngine;

public class WeatherZone : MonoBehaviour
{
    public enum WeatherType
    {
        Clear,
        Rain,
        Cloud,
        HighWind,
        RainAndWind,
        CloudAndWind
    }

    [Header("Current Weather")]
    public WeatherType currentWeather;

    [Header("Visuals")]
    public ParticleSystem rainParticles;
    public GameObject cloudVisual;
    public GameObject windVisual;

    [Header("Rain Settings")]
    public float lightRainRate = 80f;
    public float heavyRainRate = 200f;

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;

        bool hasRain = weather == WeatherType.Rain || weather == WeatherType.RainAndWind;
        bool hasCloud = weather == WeatherType.Cloud || weather == WeatherType.CloudAndWind;
        bool hasWind = weather == WeatherType.HighWind || weather == WeatherType.RainAndWind || weather == WeatherType.CloudAndWind;

        if (rainParticles != null)
        {
            var emission = rainParticles.emission;
            emission.rateOverTime = hasRain ? heavyRainRate : 0f;

            if (hasRain && !rainParticles.isPlaying)
                rainParticles.Play();

            if (!hasRain)
                rainParticles.Stop();
        }

        if (cloudVisual != null)
            cloudVisual.SetActive(hasCloud);

        if (windVisual != null)
            windVisual.SetActive(hasWind);
    }

    public void RandomizeWeather()
    {
        int count = System.Enum.GetValues(typeof(WeatherType)).Length;
        WeatherType randomWeather = (WeatherType)Random.Range(0, count);
        SetWeather(randomWeather);
    }

    void Start()
    {
        SetWeather(WeatherType.Cloud);
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RealtimeDayNightLighting : MonoBehaviour
{
    [Header("Lighting")]
    public Light sunMoonLight;

    [Header("UI")]
    public Slider timeSlider;
    public Toggle automaticTimeToggle;
    public TMP_Text timeLabel;

    [Header("Mode")]
    public bool useSlider = true;

    [Header("Light Intensity")]
    public float dayIntensity = 1.2f;
    public float nightIntensity = 0.1f;

    [Header("Light Colours")]
    public Color dayColour = new Color(1f, 0.95f, 0.85f);
    public Color nightColour = new Color(0.3f, 0.4f, 0.7f);

    [Header("Map Tint")]
    public Renderer mapRenderer;
    public Color nightMapTint = new Color(0.15f, 0.2f, 0.35f, 1f);
    public Color dayMapTint = Color.white;

    void Start()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0;
            timeSlider.maxValue = 24;
            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        }

        if (automaticTimeToggle != null)
        {
            automaticTimeToggle.isOn = !useSlider;
            automaticTimeToggle.onValueChanged.AddListener(SetAutomaticTime);
        }

        SetAutomaticTime(!useSlider);
        UpdateLighting();
    }

    void Update()
    {
        if (!useSlider)
        {
            UpdateLighting();
        }
    }

    public void OnTimeSliderChanged(float value)
    {
        if (useSlider)
        {
            UpdateLighting();
        }
    }

    public void SetAutomaticTime(bool isAutomatic)
    {
        useSlider = !isAutomatic;

        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(!isAutomatic);
        }

        if (timeLabel != null)
        {
            timeLabel.gameObject.SetActive(!isAutomatic);
        }

        UpdateLighting();
    }

    public void UpdateLighting()
    {
        if (sunMoonLight == null)
        {
            return;
        }

        float hour;

        if (useSlider && timeSlider != null)
        {
            hour = timeSlider.value;
        }
        else
        {
            DateTime now = DateTime.Now;
            hour = now.Hour + now.Minute / 60f;
        }

        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        if (timeLabel != null)
        {
            timeLabel.text = $"Time: {h:00}:{m:00}";
        }

        float dayProgress = hour / 24f;
        float daylightAmount = Mathf.SmoothStep(
            0,
            1,
            Mathf.Clamp01(Mathf.Sin(dayProgress * Mathf.PI))
        );

        sunMoonLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylightAmount);
        sunMoonLight.color = Color.Lerp(nightColour, dayColour, daylightAmount);

        RenderSettings.ambientLight = Color.Lerp(
            nightColour * 0.2f,
            dayColour * 0.7f,
            daylightAmount
        );

        float sunAngle = dayProgress * 360f - 90f;
        sunMoonLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        if (mapRenderer != null)
        {
            Color mapTint = Color.Lerp(nightMapTint, dayMapTint, daylightAmount);
            mapRenderer.material.color = mapTint;
        }
    }
}
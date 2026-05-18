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
    public Toggle manualTimeToggle;
    public TMP_Text timeLabel;

    [Header("Mode")]
    public bool useManualTime = false;

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
        useManualTime = false;

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 24f;
            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        }

        if (manualTimeToggle != null)
        {
            manualTimeToggle.isOn = useManualTime;
            manualTimeToggle.onValueChanged.AddListener(SetManualTime);
        }

        SetManualTime(useManualTime);
    }

    void Update()
    {
        if (!useManualTime)
        {
            UpdateLighting();
        }
    }

    public void OnTimeSliderChanged(float value)
    {
        if (useManualTime)
        {
            UpdateLighting();
        }
    }

    public void SetManualTime(bool isManual)
    {
        useManualTime = isManual;

        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(isManual);
        }

        if (timeLabel != null)
        {
            timeLabel.gameObject.SetActive(isManual);
        }

        UpdateLighting();
    }

    public void UpdateLighting()
    {
        if (sunMoonLight == null)
        {
            return;
        }

        float hour = GetCurrentHour();

        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        if (timeLabel != null)
        {
            timeLabel.text = $"Time: {h:00}:{m:00}";
        }

        float dayProgress = hour / 24f;

        float daylightAmount = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.Clamp01(Mathf.Sin(dayProgress * Mathf.PI))
        );

        sunMoonLight.intensity =
            Mathf.Lerp(nightIntensity, dayIntensity, daylightAmount);

        sunMoonLight.color =
            Color.Lerp(nightColour, dayColour, daylightAmount);

        RenderSettings.ambientLight = Color.Lerp(
            nightColour * 0.2f,
            dayColour * 0.7f,
            daylightAmount
        );

        float sunAngle = dayProgress * 360f - 90f;
        sunMoonLight.transform.rotation =
            Quaternion.Euler(sunAngle, 170f, 0f);

        if (mapRenderer != null)
        {
            Color mapTint =
                Color.Lerp(nightMapTint, dayMapTint, daylightAmount);

            mapRenderer.material.color = mapTint;
        }
    }

    private float GetCurrentHour()
    {
        if (useManualTime && timeSlider != null)
        {
            return timeSlider.value;
        }

        DateTime now = DateTime.Now;
        return now.Hour + now.Minute / 60f;
    }
}
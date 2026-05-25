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
    public Color nightMapTint = new Color(0.7f, 0.75f, 0.95f, 1f);
    public Color dayMapTint = Color.white;

    [Header("Map Brightness")]
    public float nightMapBrightness = 0.75f;
    public float dayMapBrightness = 4.0f;

    [Header("Cloud Tint")]
    public Renderer[] cloudRenderers;
    public Color nightCloudColour = new Color(0.35f, 0.45f, 0.7f, 1f);
    public Color dayCloudColour = Color.white;

    private Material mapMaterialInstance;
    private Material[] cloudMaterialInstances;

    void Start()
    {
        useManualTime = false;

        if (mapRenderer != null)
            mapMaterialInstance = mapRenderer.material;

        SetupCloudMaterials();

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
            UpdateLighting();
    }

    public void OnTimeSliderChanged(float value)
    {
        if (useManualTime)
            UpdateLighting();
    }

    public void SetManualTime(bool isManual)
    {
        useManualTime = isManual;

        if (timeSlider != null)
            timeSlider.gameObject.SetActive(isManual);

        if (timeLabel != null)
            timeLabel.gameObject.SetActive(isManual);

        UpdateLighting();
    }

    public void UpdateLighting()
    {
        if (sunMoonLight == null)
            return;

        float hour = GetCurrentHour();
        UpdateTimeLabel(hour);

        float daylightAmount = Mathf.Sin((hour - 6f) / 12f * Mathf.PI);
        daylightAmount = Mathf.Clamp01(daylightAmount);
        daylightAmount = Mathf.SmoothStep(0f, 1f, daylightAmount);

        sunMoonLight.intensity = Mathf.Lerp(
            nightIntensity,
            dayIntensity,
            daylightAmount
        );

        sunMoonLight.color = Color.Lerp(
            nightColour,
            dayColour,
            daylightAmount
        );

        RenderSettings.ambientLight = Color.Lerp(
            nightColour * 0.2f,
            dayColour * 0.7f,
            daylightAmount
        );

        float dayProgress = hour / 24f;
        float sunAngle = dayProgress * 360f - 90f;

        sunMoonLight.transform.rotation =
            Quaternion.Euler(sunAngle, 170f, 0f);

        UpdateMapMaterial(daylightAmount);
        UpdateCloudColours(daylightAmount);
    }

    private void UpdateMapMaterial(float daylightAmount)
    {
        if (mapMaterialInstance == null)
            return;

        float brightness = Mathf.Lerp(
            nightMapBrightness,
            dayMapBrightness,
            daylightAmount
        );

        Color mapTint = Color.Lerp(
            nightMapTint,
            dayMapTint,
            daylightAmount
        );

        mapTint *= brightness;
        mapTint.a = 1f;

        if (mapMaterialInstance.HasProperty("_BaseColor"))
            mapMaterialInstance.SetColor("_BaseColor", mapTint);
        else if (mapMaterialInstance.HasProperty("_Color"))
            mapMaterialInstance.SetColor("_Color", mapTint);
        else
            mapMaterialInstance.color = mapTint;
    }

    private void SetupCloudMaterials()
    {
        if (cloudRenderers == null)
            return;

        cloudMaterialInstances = new Material[cloudRenderers.Length];

        for (int i = 0; i < cloudRenderers.Length; i++)
        {
            if (cloudRenderers[i] != null)
                cloudMaterialInstances[i] = cloudRenderers[i].material;
        }
    }

    private void UpdateCloudColours(float daylightAmount)
    {
        if (cloudMaterialInstances == null)
            return;

        Color cloudColour = Color.Lerp(
            nightCloudColour,
            dayCloudColour,
            daylightAmount
        );

        foreach (Material mat in cloudMaterialInstances)
        {
            if (mat == null)
                continue;

            if (mat.HasProperty("_CloudsColour"))
                mat.SetColor("_CloudsColour", cloudColour);
            else if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", cloudColour);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", cloudColour);
        }
    }

    private float GetCurrentHour()
    {
        if (useManualTime && timeSlider != null)
            return timeSlider.value;

        DateTime now = DateTime.Now;
        return now.Hour + now.Minute / 60f;
    }

    private void UpdateTimeLabel(float hour)
    {
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        if (timeLabel != null)
            timeLabel.text = $"Time: {h:00}:{m:00}";
    }
}
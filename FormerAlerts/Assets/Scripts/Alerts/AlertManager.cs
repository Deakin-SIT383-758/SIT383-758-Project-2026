using UnityEngine;
using TMPro;

public class AlertManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject alertCanvas;
    public TMP_Text alertText;

    [Header("Audio")]
    public AudioSource alertAudio;

    private bool alertActive = false;

    void Start()
    {
        HideAlert();
    }

    void Update()
    {
        // TEMP TEST KEY: Press T to simulate extreme weather
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerWeatherAlert();
        }

        // TEMP TEST KEY: Press F to simulate low fuel
        if (Input.GetKeyDown(KeyCode.F))
        {
            TriggerLowFuelAlert();
        }

        // TEMP TEST KEY: Press Y to hide alert
        if (Input.GetKeyDown(KeyCode.Y))
        {
            HideAlert();
        }
    }

    public void TriggerWeatherAlert()
    {
        TriggerAlert(
            "EXTREME WEATHER WARNING",
            "The aircraft is approaching dangerous weather conditions. Review the route immediately."
        );
    }

    public void TriggerLowFuelAlert()
    {
        TriggerAlert(
            "LOW FUEL WARNING",
            "The aircraft fuel level is below the safe threshold. Check range and nearest landing options."
        );
    }

    public void TriggerAlert(string title, string message)
    {
        alertActive = true;

        if (alertCanvas != null)
        {
            alertCanvas.SetActive(true);
        }

        if (alertText != null)
        {
            alertText.text = "⚠ " + title + "\n" + message;
        }

        if (alertAudio != null)
        {
            alertAudio.Stop(); // prevents overlapping sounds
            alertAudio.Play();
        }

        Debug.Log("Alert Triggered: " + title + " - " + message);
    }

    public void HideAlert()
    {
        alertActive = false;

        if (alertCanvas != null)
        {
            alertCanvas.SetActive(false);
        }

        if (alertAudio != null)
        {
            alertAudio.Stop();
        }

        Debug.Log("Alert Hidden");
    }
}
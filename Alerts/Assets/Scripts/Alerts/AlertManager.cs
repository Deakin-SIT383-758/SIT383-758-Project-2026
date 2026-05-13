using UnityEngine;
using TMPro;

public class AlertManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject alertCanvas;
    public TMP_Text alertText;

    [Header("Audio")]
    public AudioSource alertAudio;

    void Start()
    {
        HideAlert();
    }

    void Update()
    {
        // KEYBOARD TESTING IN UNITY EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerWeatherAlert();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TriggerLowFuelAlert();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            HideAlert();
        }

        // QUEST CONTROLLER TESTING
        // A button = weather warning
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            TriggerWeatherAlert();
        }

        // B button = low fuel warning
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            TriggerLowFuelAlert();
        }

        // Y button = hide warning
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
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
            alertAudio.Stop();
            alertAudio.Play();
        }

        Debug.Log("Alert Triggered: " + title + " - " + message);
    }

    public void HideAlert()
    {
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
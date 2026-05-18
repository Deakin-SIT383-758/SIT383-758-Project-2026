using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class AlertManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject alertCanvas;
    public TMP_Text alertText;

    [Header("Audio")]
    public AudioSource alertAudio;

    [Header("OpenXR / XRI Input")]
    public InputActionProperty weatherAlertAction;
    public InputActionProperty lowFuelAlertAction;
    public InputActionProperty hideAlertAction;

    private void OnEnable()
    {
        RegisterAction(weatherAlertAction, OnWeatherAlertPressed);
        RegisterAction(lowFuelAlertAction, OnLowFuelAlertPressed);
        RegisterAction(hideAlertAction, OnHideAlertPressed);
    }

    private void OnDisable()
    {
        UnregisterAction(weatherAlertAction, OnWeatherAlertPressed);
        UnregisterAction(lowFuelAlertAction, OnLowFuelAlertPressed);
        UnregisterAction(hideAlertAction, OnHideAlertPressed);
    }

    private void Start()
    {
        HideAlert();
    }

    private void Update()
    {
        // Keyboard testing in Unity Editor using the new Input System
        if (Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            TriggerWeatherAlert();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TriggerLowFuelAlert();
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            HideAlert();
        }
    }

    private void RegisterAction(InputActionProperty actionProperty, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionProperty.action == null) return;

        actionProperty.action.Enable();
        actionProperty.action.performed += callback;
    }

    private void UnregisterAction(InputActionProperty actionProperty, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionProperty.action == null) return;

        actionProperty.action.performed -= callback;
        actionProperty.action.Disable();
    }

    private void OnWeatherAlertPressed(InputAction.CallbackContext context)
    {
        TriggerWeatherAlert();
    }

    private void OnLowFuelAlertPressed(InputAction.CallbackContext context)
    {
        TriggerLowFuelAlert();
    }

    private void OnHideAlertPressed(InputAction.CallbackContext context)
    {
        HideAlert();
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
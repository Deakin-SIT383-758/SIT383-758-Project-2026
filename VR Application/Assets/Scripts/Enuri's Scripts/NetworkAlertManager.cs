using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NetworkAlertManager : NetworkBehaviour
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
        HideAlertLocal();
    }

    private void Update()
    {
        // Keyboard fallbacks for Unity Editor testing
        if (Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            RPC_ShowWeatherAlert();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            RPC_ShowLowFuelAlert();
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            RPC_HideAlert();
        }
    }

    private void RegisterAction(
        InputActionProperty actionProperty,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionProperty.action == null) return;

        actionProperty.action.Enable();
        actionProperty.action.performed += callback;
    }

    private void UnregisterAction(
        InputActionProperty actionProperty,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionProperty.action == null) return;

        actionProperty.action.performed -= callback;
        actionProperty.action.Disable();
    }

    private void OnWeatherAlertPressed(InputAction.CallbackContext context)
    {
        RPC_ShowWeatherAlert();
    }

    private void OnLowFuelAlertPressed(InputAction.CallbackContext context)
    {
        RPC_ShowLowFuelAlert();
    }

    private void OnHideAlertPressed(InputAction.CallbackContext context)
    {
        RPC_HideAlert();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShowWeatherAlert()
    {
        ShowAlertLocal(
            "EXTREME WEATHER WARNING",
            "The aircraft is approaching dangerous weather conditions. Review the route immediately."
        );
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShowLowFuelAlert()
    {
        ShowAlertLocal(
            "LOW FUEL WARNING",
            "The aircraft fuel level is below the safe threshold. Check range and nearest landing options."
        );
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HideAlert()
    {
        HideAlertLocal();
    }

    private void ShowAlertLocal(string title, string message)
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

        Debug.Log("Network alert shown: " + title);
    }

    private void HideAlertLocal()
    {
        if (alertCanvas != null)
        {
            alertCanvas.SetActive(false);
        }

        if (alertAudio != null)
        {
            alertAudio.Stop();
        }

        Debug.Log("Network alert hidden");
    }
}
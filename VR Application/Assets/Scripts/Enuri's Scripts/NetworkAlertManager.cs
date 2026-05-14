using Fusion;
using UnityEngine;
using TMPro;

public class NetworkAlertManager : NetworkBehaviour
{
    [Header("UI")]
    public GameObject alertCanvas;
    public TMP_Text alertText;

    [Header("Audio")]
    public AudioSource alertAudio;

    private void Start()
    {
        HideAlertLocal();
    }

    private void Update()
    {
        // Right controller A = weather warning
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            RPC_ShowWeatherAlert();
        }

        // Right controller B = low fuel warning
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            RPC_ShowLowFuelAlert();
        }

        // Left controller Y = hide warning
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            RPC_HideAlert();
        }

        // Keyboard fallbacks for Unity Editor testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            RPC_ShowWeatherAlert();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            RPC_ShowLowFuelAlert();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            RPC_HideAlert();
        }
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
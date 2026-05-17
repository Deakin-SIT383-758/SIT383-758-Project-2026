using Fusion;
using Photon.Voice.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkButton : NetworkBehaviour
{
    [Header("References")]
    public Transform buttonTop;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    [Header("Animation")]
    public float pressDepth = 0.5f;
    public float pressSpeed = 10f;
    public float returnDelay = 0.1f;

    private Vector3 startPos;
    private bool isPressed = false;

    public string controlName;
    public bool isToggle = false; // Is this button a toggle?
    private bool toggleState = false; // On or off state of toggle
    public GameObject toggleLight; // Light gameobject to toggle
    [Header("Audio")]
    public AudioSource engineStartSource;
    public AudioSource engineIdleSource;
    public bool isAudio = false;
    public bool isPlaying = false;
    public bool isIntercom = false;
    public Recorder Intercom;
    public bool muted = false;

    private void Start()
    {
        buttonTop = transform.Find("Button");
        if (isToggle)
        {
            pressDepth = .25f;
            toggleLight = transform.Find("Button").transform.Find("Light").gameObject;
            toggleLight.SetActive(false);

        }
        if (isAudio)
        {
            engineStartSource.Stop();
            engineIdleSource.Stop();

        }


        startPos = buttonTop.localPosition;
        interactable = this.gameObject.GetComponent<XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnPress);
        if (toggleLight != null)
        {
            toggleLight.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnPress);
    }

    private void OnPress(SelectEnterEventArgs args)
    {
        if (!isPressed)
        {
            RPC_PressButton();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PressButton()
    {
        StartCoroutine(PressAnimation());
    }
    IEnumerator PressAnimation()
    {
        isPressed = true;

        Vector3 pressedPos = startPos + Vector3.down * pressDepth;

        // Move down
        while (Vector3.Distance(buttonTop.localPosition, pressedPos) > 0.001f)
        {
            buttonTop.localPosition = Vector3.Lerp(
                buttonTop.localPosition,
                pressedPos,
                Time.deltaTime * pressSpeed
            );

            yield return null;
        }
        if (isAudio && !isPlaying)
        {
            isPlaying = true;
            // TURN ON ENGINE
            if (engineStartSource != null)
            {
                engineStartSource.Play();
                yield return new WaitForSeconds(engineStartSource.clip.length);
            }

            if (engineIdleSource != null)
            {
                engineIdleSource.loop = true;
                engineIdleSource.Play();
            }
            ChecklistManager.Instance.ControlUpdate(controlName, 1);
        }
        else if (isAudio)
        {
            // TURN OFF ENGINE
            if (engineStartSource != null)
                engineStartSource.Stop();

            if (engineIdleSource != null)
                engineIdleSource.loop = false;
                engineIdleSource.Stop();
            isPlaying = false;
        }
        if (isIntercom)
        {
            if (muted)
            {
                Intercom.RecordingEnabled = true;
                muted = false;
            }
            else
            {
                Intercom.RecordingEnabled = false;
                muted = true;
            }
        }
        // ===== ACTION HERE =====
        Debug.Log("VR Button Pressed!");
        if (isToggle)
        {
            toggleState = !toggleState; // Swap state
            int i = toggleState ? 1 : 0;
            toggleLight.SetActive(toggleState);
            ChecklistManager.Instance.ControlUpdate(controlName, i);
        }
        else
        {
            ChecklistManager.Instance.ControlUpdate(controlName, 1);
        }

        yield return new WaitForSeconds(returnDelay);

        // Move back up
        while (Vector3.Distance(buttonTop.localPosition, startPos) > 0.001f)
        {
            buttonTop.localPosition = Vector3.Lerp(
                buttonTop.localPosition,
                startPos,
                Time.deltaTime * pressSpeed
            );

            yield return null;
        }

        buttonTop.localPosition = startPos;

        isPressed = false;
    }
}


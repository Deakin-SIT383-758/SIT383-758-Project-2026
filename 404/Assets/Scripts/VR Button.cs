using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class VRButton : MonoBehaviour
{
    [Header("References")]
    public Transform buttonTop;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    [Header("Animation")]
    public float pressDepth = 0.02f;
    public float pressSpeed = 10f;
    public float returnDelay = 0.1f;

    private Vector3 startPos;
    private bool isPressed = false;

    [Header("Toggle")]
    public string controlName;
    public bool isToggle = false;
    private bool toggleState = false;
    public GameObject toggleLight;

    [Header("Audio")]
    public AudioSource engineStartSource;
    public AudioSource engineIdleSource;

    private void Start()
    {
        startPos = buttonTop.localPosition;

        interactable.selectEntered.AddListener(OnPress);

        if (toggleLight != null)
            toggleLight.SetActive(false);

        // Ensure audio is OFF at start
        if (engineStartSource != null)
            engineStartSource.Stop();

        if (engineIdleSource != null)
        {
            engineIdleSource.Stop();
            engineIdleSource.loop = true;
        }
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnPress);
    }

    private void OnPress(SelectEnterEventArgs args)
    {
        if (!isPressed)
            StartCoroutine(PressAnimation());
    }

    private IEnumerator PressAnimation()
    {
        isPressed = true;

        Vector3 pressedPos = startPos + Vector3.down * pressDepth;

        // PRESS DOWN
        while (Vector3.Distance(buttonTop.localPosition, pressedPos) > 0.001f)
        {
            buttonTop.localPosition = Vector3.Lerp(
                buttonTop.localPosition,
                pressedPos,
                Time.deltaTime * pressSpeed
            );
            yield return null;
        }

        buttonTop.localPosition = pressedPos;

        // =========================
        // TOGGLE LOGIC (THIS IS KEY)
        // =========================
        if (isToggle)
        {
            toggleState = !toggleState;

            if (toggleLight != null)
                toggleLight.SetActive(toggleState);

            if (toggleState)
            {
                // TURN ON ENGINE
                if (engineStartSource != null)
                {
                    engineStartSource.Play();
                    yield return new WaitForSeconds(engineStartSource.clip.length);
                }

                if (engineIdleSource != null)
                    engineIdleSource.Play();
            }
            else
            {
                // TURN OFF ENGINE
                if (engineStartSource != null)
                    engineStartSource.Stop();

                if (engineIdleSource != null)
                    engineIdleSource.Stop();
            }
        }

        yield return new WaitForSeconds(returnDelay);

        // RETURN UP
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
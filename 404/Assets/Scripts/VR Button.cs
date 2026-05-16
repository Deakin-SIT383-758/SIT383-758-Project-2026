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

    public string controlName;
    public bool isToggle = false; // Is this button a toggle?
    private bool toggleState = false; // On or off state of toggle
    public GameObject toggleLight; // Light gameobject to toggle

    private void Start()
    {
        startPos = buttonTop.localPosition;

        interactable.selectEntered.AddListener(OnPress);

        toggleLight.SetActive(false);
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnPress);
    }

    private void OnPress(SelectEnterEventArgs args)
    {
        if (!isPressed)
        {
            StartCoroutine(PressAnimation());
        }
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
using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;

    [Header("XR Input Action")]
    [SerializeField] private InputActionProperty menuButton;

    private bool wasPressedLastFrame;

    private void OnEnable()
    {
        if (menuButton.action != null)
        {
            menuButton.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (menuButton.action != null)
        {
            menuButton.action.Disable();
        }
    }

    private void Update()
    {
        if (menuButton.action == null || menuRoot == null)
            return;

        bool isPressed =
            menuButton.action.WasPressedThisFrame();

        if (isPressed && !wasPressedLastFrame)
        {
            menuRoot.SetActive(!menuRoot.activeSelf);
        }

        wasPressedLastFrame = isPressed;
    }
}
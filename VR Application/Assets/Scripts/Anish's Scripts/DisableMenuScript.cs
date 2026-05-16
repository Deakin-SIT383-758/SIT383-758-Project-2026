using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    public GameObject menuRoot;

    [Header("OpenXR / XRI Input")]
    public InputActionProperty menuButton;

    private void OnEnable()
    {
        menuButton.action.Enable();
        menuButton.action.performed += ToggleMenu;
    }

    private void OnDisable()
    {
        menuButton.action.performed -= ToggleMenu;
        menuButton.action.Disable();
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        if (menuRoot == null) return;

        menuRoot.SetActive(!menuRoot.activeSelf);
    }
}
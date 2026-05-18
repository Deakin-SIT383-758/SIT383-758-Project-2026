using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private InputActionProperty menuButton;

    private void OnEnable() => menuButton.action?.Enable();
    private void OnDisable() => menuButton.action?.Disable();

    private void Update()
    {
        if (menuButton.action == null || menuRoot == null) return;

        if (menuButton.action.WasPressedThisFrame())
        {
            menuRoot.SetActive(!menuRoot.activeSelf);
        }
    }
}
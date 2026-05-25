using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary free-fly camera for inspecting the scene during development.
/// WASD = move, Q/E = down/up, Shift = boost, mouse = look.
/// Right-click and hold to enable mouse look (so the cursor stays free otherwise).
/// Uses the new Input System.
/// </summary>
public class FlyCamController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float boostMultiplier = 4f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float pitchClamp = 89f;
    [Tooltip("If true, mouse look is always active. If false, hold right mouse button to look.")]
    [SerializeField] private bool alwaysLook = false;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Vector3 e = transform.eulerAngles;
        _yaw = e.y;
        _pitch = e.x;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
    }

    private void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        bool looking = alwaysLook || mouse.rightButton.isPressed;
        if (!looking)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Vector2 delta = mouse.delta.ReadValue();
        _yaw   += delta.x * lookSensitivity;
        _pitch -= delta.y * lookSensitivity;
        _pitch  = Mathf.Clamp(_pitch, -pitchClamp, pitchClamp);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void HandleMove()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir += transform.forward;
        if (kb.sKey.isPressed) dir -= transform.forward;
        if (kb.aKey.isPressed) dir -= transform.right;
        if (kb.dKey.isPressed) dir += transform.right;
        if (kb.eKey.isPressed) dir += Vector3.up;
        if (kb.qKey.isPressed) dir -= Vector3.up;

        if (dir.sqrMagnitude < 0.0001f) return;

        float speed = moveSpeed * (kb.leftShiftKey.isPressed ? boostMultiplier : 1f);
        transform.position += dir.normalized * speed * Time.deltaTime;
    }
}